using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

public class HumanFoot : MonoBehaviour
{
    private Animator _animator;
    public PolygonCollider2D _legCollider;

    [SerializeField] private Color _colorA;
    [SerializeField] private Color _colorB;

    [SerializeField] private GameObject _pincerPrefab;

    [SerializeField] private AudioSource moveSound;
    [SerializeField] private AudioSource stepSound;

    private Rigidbody2D _legRigidbody;

    private float _exitTimer;

    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
        _legRigidbody = _legCollider.GetComponent<Rigidbody2D>();

        _exitTimer = Random.Range(12f, 25f);

        _legCollider.GetComponent<SpriteShapeRenderer>().color = Color.Lerp(_colorA, _colorB, Random.Range(0f, 1f));
    }

    // Update is called once per frame
    void Update()
    {
        _exitTimer -= Time.deltaTime;

        moveSound.volume = _legRigidbody.velocity.magnitude;

        if (_exitTimer <= 0)
        {
            _animator.SetTrigger("Leave");

            StartCoroutine(DestroyFootAfterAnimation());

            _exitTimer = 1203912f;
        }
    }

    public void PlayStepSound()
    {
        stepSound.Play();
    }

    private IEnumerator DestroyFootAfterAnimation()
    {
        yield return new WaitForSeconds(10f);

        // Check if player is a child of the foot
        if (Tick.Instance.transform.parent == _legCollider.transform)
        {
            // We spawn a pincer
            Instantiate(_pincerPrefab, Tick.Instance.transform.position, Quaternion.identity);

            yield return new WaitForSeconds(7.8f);
            _legCollider.enabled = false;

            if (Tick.Instance.transform.parent == _legCollider.transform)
            {
                Tick.Instance.TakeDamage(15f * Tick.Instance.timesEvolved);
                Tick.Instance.StopSuckBlood();
            }

            // Check again after 10 seconds
            yield return new WaitForSeconds(5f);
        }

        FootSpawner.Instance.foots.Remove(gameObject);

        Destroy(gameObject);
    }
}
