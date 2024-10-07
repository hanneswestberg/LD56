using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class BabyTick : MonoBehaviour
{
    private Animator animator;
    private Rigidbody2D rigidBody;
    private CircleCollider2D circleCollider;

    private Vector2 _desiredMovement;
    private float _grip = 1f;
    private float _gravity;

    private SpriteShapeController _nearestGrass;

    private int _groundLayer;
    private int _walkableGroundLayer;
    private int _grassLayer;

    private float checkNearestGrassTimer = 0f;
    private Vector2? _nearestGrassPoint;

    // Spline navigation variables
    private int _currentCurveIndex;
    private float _currentT;

    // Closest point variables
    private int _closestCurveIndex;
    private float _closestT;

    // Movement parameters
    public float movementSpeed = 0.5f;     // Adjust this to change the movement speed along the spline
    public float acceleration = 5f;      // Adjust this for acceleration smoothing
    public float rotationSpeed = 5f;     // Adjust this for rotation smoothing

    private float spawnTimer = 0f;

    void Start()
    {
        animator = GetComponent<Animator>();
        rigidBody = GetComponent<Rigidbody2D>();
        circleCollider = GetComponent<CircleCollider2D>();

        _grassLayer = LayerMask.GetMask("Grass");
        _walkableGroundLayer = LayerMask.GetMask("WalkableGround");
        _groundLayer = LayerMask.GetMask("Ground");

        spawnTimer = 0f;
    }

    void Update()
    {
        animator.SetFloat("Speed", _desiredMovement.magnitude);
        animator.SetBool("Falling", _grip < 0.1f);

        checkNearestGrassTimer -= Time.deltaTime;

        if (checkNearestGrassTimer <= 0f)
        {
            SetNearestGrass();
        }

        spawnTimer -= Time.deltaTime;

        CalculateNearestGrassPoint();
        Rotate();
    }

    private void SetNearestGrass()
    {
        checkNearestGrassTimer = 1f; // Update more frequently

        float detectionRadius = 5f; // Adjust as needed
        Collider2D[] grassColliders = Physics2D.OverlapCircleAll(transform.position, detectionRadius, _grassLayer);

        if (grassColliders.Length == 0)
        {
            _nearestGrass = null;
            return;
        }

        float closestDistance = float.MaxValue;
        SpriteShapeController closestGrass = null;
        Vector3 closestPoint = Vector3.zero;

        foreach (var collider in grassColliders)
        {
            var grass = collider.GetComponent<SpriteShapeController>();
            if (grass == null)
                continue;

            // Get the closest point on the spline to the tick
            Vector3 pointOnSpline;
            float distance = GetClosestPointOnSpline(grass.spline, transform.position, out pointOnSpline);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestGrass = grass;
                closestPoint = pointOnSpline;
                _currentCurveIndex = _closestCurveIndex; // Store the curve index
                _currentT = _closestT;                   // Store the t parameter
            }
        }

        _nearestGrass = closestGrass;
        _nearestGrassPoint = closestPoint;
    }

    private float GetClosestPointOnSpline(Spline spline, Vector2 position, out Vector3 closestPoint)
    {
        float minDistance = float.MaxValue;
        closestPoint = Vector3.zero;
        _closestCurveIndex = 0;
        _closestT = 0f;

        int pointCount = spline.GetPointCount();

        // Iterate through the spline's segments
        for (int i = 0; i < pointCount - 1; i++)
        {
            // Sample points along the curve between point i and point i + 1
            for (float t = 0f; t <= 1f; t += 0.1f) // Adjust sampling rate as needed
            {
                Vector3 pointOnCurve = EvaluateSpline(spline, i, t);
                float distance = Vector2.Distance(position, pointOnCurve);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestPoint = pointOnCurve;
                    _closestCurveIndex = i;
                    _closestT = t;
                }
            }
        }

        return minDistance;
    }

    private Vector3 EvaluateSpline(Spline spline, int index, float t)
    {
        int pointCount = spline.GetPointCount();
        int nextIndex = index + 1;

        if (nextIndex >= pointCount)
        {
            if (spline.isOpenEnded)
            {
                nextIndex = index; // Stay at the last point
                t = 1f;
            }
            else
            {
                nextIndex = 0; // Loop back to the start if the spline is closed
            }
        }

        // Get control points and tangents for Bezier curve
        Vector3 p0 = spline.GetPosition(index);
        Vector3 p1 = p0 + spline.GetRightTangent(index);
        Vector3 p3 = spline.GetPosition(nextIndex);
        Vector3 p2 = p3 + spline.GetLeftTangent(nextIndex);

        // Cubic Bezier curve formula
        Vector3 point = Mathf.Pow(1 - t, 3) * p0
                      + 3 * Mathf.Pow(1 - t, 2) * t * p1
                      + 3 * (1 - t) * Mathf.Pow(t, 2) * p2
                      + Mathf.Pow(t, 3) * p3;

        return point;
    }

    private void CalculateNearestGrassPoint()
    {
        if (_nearestGrass == null)
        {
            _desiredMovement = Vector2.zero;
            return;
        }

        Spline spline = _nearestGrass.spline;

        // Move along the spline
        _currentT += Time.deltaTime * movementSpeed;

        while (_currentT > 1f)
        {
            _currentT -= 1f;
            _currentCurveIndex++;

            if (_currentCurveIndex >= spline.GetPointCount())
            {
                if (spline.isOpenEnded)
                {
                    // Stay at the end of the spline
                    _currentCurveIndex = spline.GetPointCount() - 1;
                    _currentT = 1f;
                    break;
                }
                else
                {
                    // Loop back to the start if the spline is closed
                    _currentCurveIndex = 0;
                }
            }
        }

        Vector3 targetPosition = EvaluateSpline(spline, _currentCurveIndex, _currentT);

        if (_currentCurveIndex == spline.GetPointCount() - 1 && _currentT >= 1f && spline.isOpenEnded)
        {
            // Continue moving in the same direction as the last segment
            Vector3 lastPoint = spline.GetPosition(_currentCurveIndex);
            Vector3 prevPoint = spline.GetPosition(_currentCurveIndex - 1);
            Vector3 direction = (lastPoint - prevPoint).normalized;
            _desiredMovement = direction;
        }
        else
        {
            _desiredMovement = (targetPosition - transform.position).normalized;
        }
    }

    void FixedUpdate()
    {
        if (spawnTimer > 0f)
            return;

        bool isTouchingWalkable = circleCollider.IsTouchingLayers(_grassLayer) || circleCollider.IsTouchingLayers(_walkableGroundLayer);
        bool isTouchingGround = circleCollider.IsTouchingLayers(_groundLayer);

        if (isTouchingWalkable || isTouchingGround)
        {
            _gravity = Mathf.Lerp(_gravity, 0f, Time.fixedDeltaTime * 5f);
            _grip = Mathf.Lerp(_grip, 1f, Time.fixedDeltaTime * 5f);

            Vector2 velocity = _desiredMovement * movementSpeed * _grip;
            rigidBody.velocity = Vector2.Lerp(rigidBody.velocity, velocity, Time.fixedDeltaTime * acceleration);
        }
        else
        {
            _gravity = 1f;
            _grip = 0f;
        }

        rigidBody.gravityScale = _gravity;
    }

    private void Rotate()
    {
        if (_desiredMovement.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(_desiredMovement.y, _desiredMovement.x) * Mathf.Rad2Deg - 90f;
            Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }
}
