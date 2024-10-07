using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

public class Tick : MonoBehaviour
{
    public static Tick Instance;

    public float currentBlood = 50;
    public float maxBlood = 100;
    public float suckRate = 5f;
    public bool isSuckingBlood = false;
    public bool isEvolving = false;
    public bool canSuckBlood = false;
    public bool canEvolve = false;
    public float Score { get; private set; } = 0;

    public int BabiesSpawned { get; private set; } = 0;

    public int timesEvolved = 1;

    [SerializeField] private float bloodDrainRate = 0.3f;
    [SerializeField] private float startingSpeed = 1.4f;

    [SerializeField] private ParticleSystem damageParticles;
    [SerializeField] private ParticleSystem suckParticles;
    [SerializeField] private ParticleSystem evolveParticles;
    [SerializeField] private ParticleSystem evolveFinishedParticles;
    [SerializeField] private GameObject bitePrefab;
    [SerializeField] private GameObject babyTickPrefab;

    private Rigidbody2D rigidBody;
    private CircleCollider2D circleCollider;
    private Animator animator;

    private SortingGroup sortingGroup;
    [SerializeField] private SpriteRenderer[] tickSprites;

    [SerializeField] private AudioSource jumpSound;
    [SerializeField] private AudioSource suckSound;
    [SerializeField] private AudioSource walkSound;
    [SerializeField] private AudioSource fallSound;
    [SerializeField] private AudioSource evolveSound;
    [SerializeField] private AudioSource spawnSound;
    [SerializeField] private AudioSource evolveFinishedSound;
    [SerializeField] private AudioSource fallBlood;
    [SerializeField] private AudioSource biteSound;

    private Vector2 _desiredMovement;
    private Vector2 _lastMovement;

    private float _gravity;
    private float _grip = 1;
    private float _lookAngle;
    private float _jumpDelay;

    private int _humanLayer;
    private int _grassLayer;
    private int _walkableGroundLayer;
    private int _groundLayer;

    private Camera _camera;

    private bool _isTouchingHuman;
    private bool _isTouchingWalkable;
    private bool _isTouchingGround;
    private bool _isSpawning;

    private Vector3 startingScale;

    private bool _gameIsStarted;

    private float walkSoundInterval = 0.2f;
    private float movementDelay = 0f;

    private void Awake()
    {
        if(Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        circleCollider = GetComponent<CircleCollider2D>();
        animator = GetComponent<Animator>();
        _camera = Camera.main;
        sortingGroup = GetComponentInChildren<SortingGroup>();

        rigidBody.bodyType = RigidbodyType2D.Kinematic;

        sortingGroup.sortingOrder = 100;
        startingScale = sortingGroup.transform.localScale;

        _humanLayer = LayerMask.GetMask("Human");
        _grassLayer = LayerMask.GetMask("Grass");
        _walkableGroundLayer = LayerMask.GetMask("WalkableGround");
        _groundLayer = LayerMask.GetMask("Ground");

        sortingGroup.gameObject.SetActive(false);

        _isSpawning = true;
    }

    public void StartGame()
    {
        sortingGroup.gameObject.SetActive(true);
        _gameIsStarted = true;
    }

    void Update()
    {
        if(!_gameIsStarted || isEvolving)
            return;

        Move();
        Rotate();

        animator.SetFloat("Speed", _desiredMovement.magnitude);
        animator.SetBool("Falling", _grip < 0.1f);
        animator.SetBool("Eating", isSuckingBlood);

        if (isSuckingBlood)
        {
            currentBlood = Mathf.Clamp(currentBlood + suckRate * Time.deltaTime, 0, maxBlood);
            Score += suckRate * Time.deltaTime;
        }
        else
        {
            currentBlood -= Time.deltaTime * bloodDrainRate;
            if (currentBlood <= 0)
            {
                // Game Over
                GameManager.Instance.LoseGame();
                _gameIsStarted = false;
            }
        }

        canEvolve = currentBlood >= maxBlood * 0.8f && _isTouchingGround;

        _jumpDelay -= Time.deltaTime;
        movementDelay -= Time.deltaTime;
        walkSoundInterval -= Time.deltaTime;
    }

    private void Rotate()
    {
        if(movementDelay > 0)
            return;

        _lookAngle = -90 + Mathf.Atan2(_desiredMovement.y, _desiredMovement.x) * Mathf.Rad2Deg;

        if (_desiredMovement.magnitude > 0.1f)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, _lookAngle), 0.04f);

            if(isSuckingBlood)
                StopSuckBlood();
        }
    }

    public void Spawn()
    {
        animator.SetTrigger("Spawn");
        evolveSound.Play();
    }

    public void SpawnComplete()
    {
        _isSpawning = false;
        spawnSound.Play();
        evolveSound.Stop();
        rigidBody.bodyType = RigidbodyType2D.Dynamic;
    }

    private void Move()
    {
        if(movementDelay > 0)
            return;

        if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
        {
            var moveVector = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized;
            _desiredMovement = moveVector * startingSpeed;
            _lastMovement = moveVector;
        }
        // You can also move by clicking on the screen and dragging
        else if (Input.touches.Length > 0)
        {
            var input = Input.touches.FirstOrDefault();

            var inputPosition = _camera.ScreenToWorldPoint(input.position).ToVector2();
            var moveVector = (inputPosition - transform.position.ToVector2()).normalized;

            _desiredMovement = moveVector.normalized * startingSpeed;
            _lastMovement = moveVector;
        }
        else
        {
            _desiredMovement = Vector2.Lerp(_desiredMovement, Vector2.zero, 0.1f);
        }

        if(_desiredMovement.magnitude > 0.1f && walkSoundInterval <= 0 && _grip > 0.1f)
        {
            walkSound.pitch = Random.Range(1.5f, 1.8f) * Mathf.Max(_grip, 0.3f);
            walkSound.volume = Random.Range(0.1f, 0.25f);
            walkSound.Play();

            // Faster the better the grip
            walkSoundInterval = Random.Range(0.05f, 0.1f) / Mathf.Max(_grip, 0.3f);
        }

        if(Input.GetKey(KeyCode.Space))
            Jump();
    }

    public void TakeDamage(float damage)
    {
        damageParticles.Play();
        fallSound.Play();
        fallBlood.Play();
        currentBlood = Mathf.Clamp(currentBlood - damage, 0, maxBlood);
    }

    public void Jump()
    {
        if (!(_grip > 0.1f) || !(_jumpDelay <= 0))
            return;

        var jumpDirection = _lastMovement.normalized;

        if(jumpDirection.y < 0)
            jumpDirection.y = -jumpDirection.y;

        rigidBody.AddForce(jumpDirection * 6f, ForceMode2D.Impulse);
        _jumpDelay = 0.5f;

        jumpSound.Play();
    }

    void FixedUpdate()
    {
        if(!_gameIsStarted || isEvolving || _isSpawning || movementDelay > 0)
            return;

        _isTouchingHuman = circleCollider.IsTouchingLayers(_humanLayer);
        _isTouchingWalkable = circleCollider.IsTouchingLayers(_grassLayer) || circleCollider.IsTouchingLayers(_walkableGroundLayer);
        _isTouchingGround = circleCollider.IsTouchingLayers(_groundLayer) || circleCollider.IsTouchingLayers(_walkableGroundLayer);

        var canWalkOnHuman = _isTouchingHuman && !_isTouchingWalkable;
        var isWalkingOnHuman = transform.parent != null;
        var isJumping = _jumpDelay > 0;

        if(canWalkOnHuman && !isWalkingOnHuman)
        {
            // Get the human that the tick is touching
            var human = Physics2D.OverlapCircle(transform.position, 0.2f, _humanLayer);
            if (human != null)
            {
                // make the tick child of the human
                transform.SetParent(human.transform);

                var humanSortingGroup = human.transform.GetComponentInParent<SortingGroup>();
                humanSortingGroup.sortingOrder = 1000;
            }
        }
        else if(!canWalkOnHuman && isWalkingOnHuman && !isSuckingBlood)
        {
            transform.SetParent(null);
        }

        canSuckBlood = _isTouchingHuman && !_isTouchingWalkable;

        if (!isJumping && (_isTouchingHuman || _isTouchingWalkable || _isTouchingGround))
        {
            _gravity = Mathf.Lerp(_gravity, 0, 0.015f);
            _grip = Mathf.Lerp(_grip, 1, 0.015f);
            rigidBody.velocity = Vector2.Lerp(rigidBody.velocity, _desiredMovement * _grip, _grip);
        }
        else
        {
            _gravity = 1;
            _grip = 0;
        }

        rigidBody.gravityScale = _gravity;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        var contacts = new ContactPoint2D[col.contactCount];
        col.GetContacts(contacts);

        var totalImpulse = 0f;

        foreach (var contact in contacts) {
            totalImpulse += contact.normalImpulse;
        }

        if(totalImpulse < 8f)
            return;

        TakeDamage(totalImpulse * timesEvolved / 1.3f);
    }

    public void DelayedEvolve()
    {
        // Evolve
        maxBlood += 50;
        suckRate += 2f;
        bloodDrainRate += 0.15f;
        startingSpeed += 0.2f;
        currentBlood -= currentBlood * 0.5f;
        Score += 100;

        sortingGroup.transform.localScale = startingScale + startingScale * (timesEvolved * 0.15f);

        foreach (var tickSprite in tickSprites)
        {
            tickSprite.color = Color.Lerp(tickSprite.color, Color.white, 0.1f);
        }

        // var babyTickSpawnPos = transform.position.ToVector2() + Vector2.down * 0.5f;
        var spawnedBabies = 3;

        for (var i = 0; i < spawnedBabies; i++)
        {

            var babytick = Instantiate(babyTickPrefab, transform.position, Quaternion.identity);

            babytick.GetComponent<Rigidbody2D>().AddForce(Vector2.up * Random.Range(4f, 8f) + new Vector2(Random.Range(-1f, 1f), 0f) * Random.Range(4f, 8f), ForceMode2D.Impulse);

            BabiesSpawned++;
        }

        timesEvolved++;

        rigidBody.mass += 0.15f;
        circleCollider.radius = 0.2f + timesEvolved * 0.01f;

        isEvolving = false;

        evolveFinishedParticles.Play();
        evolveFinishedSound.Play();

        evolveSound.Stop();
        evolveParticles.Stop();
    }

    public void Evolve()
    {
        // We can evolve if the current blood is more than 80% of the max blood
        if(currentBlood < maxBlood * 0.8f || !_isTouchingGround)
            return;

        movementDelay = 0.5f;
        _desiredMovement = Vector2.zero;
        rigidBody.velocity = Vector2.zero;

        isEvolving = true;

        animator.SetTrigger("Evolve");

        evolveSound.Play();
        evolveParticles.Play();
    }

    public void StopSuckBlood()
    {
        isSuckingBlood = false;
        suckParticles.Stop();
        sortingGroup.sortingOrder = 100;
        suckSound.Stop();
    }

    public void SuckBlood()
    {
        if(isSuckingBlood)
            return;

        biteSound.Play();

        movementDelay = 0.2f;

        _grip = 1;
        suckSound.Play();
        isSuckingBlood = true;
        suckParticles.Play();
        sortingGroup.sortingOrder = -100;

        _desiredMovement = Vector2.zero;
        rigidBody.velocity = Vector2.zero;

        // Spawn a bite prefab on the human
        var bite = Instantiate(bitePrefab, transform.position, Quaternion.identity, transform.parent);

        // Rotate the bite to the direction of the tick
        bite.transform.right = _lastMovement;
    }
}
