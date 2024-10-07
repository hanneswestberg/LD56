using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class GrassStraw : MonoBehaviour
{
    private SpriteShapeController _spriteShapeController;
    private SpriteShapeRenderer _spriteShapeRenderer;

    [SerializeField] private Color colorA;
    [SerializeField] private Color colorB;
    [SerializeField] private int minLength = 4;
    [SerializeField] private int maxLength = 12;
    [SerializeField] private float startHeight = 2;

    private float _startXPosition;
    private Vector3[] _grassVectors;
    private Vector3 _windDirection;
    private float _windChangeTimer;
    private float _windSpeed = 0.01f;

    // Start is called before the first frame update
    void Start()
    {
        _spriteShapeController = GetComponent<SpriteShapeController>();
        _spriteShapeRenderer = GetComponent<SpriteShapeRenderer>();

        _startXPosition = transform.position.x;

        var color = Color.Lerp(colorA, colorB, Random.Range(0f, 1f));
        _spriteShapeRenderer.color = color;

        CreateGrass();

        UpdateWindDirection();
    }

    private void UpdateWindDirection()
    {
        _windDirection = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0).normalized;
        _windChangeTimer = Random.Range(5f, 10f);
        _windSpeed = Random.Range(0.01f, 0.03f);
    }

    private void CreateGrass()
    {
        _spriteShapeController.spline.Clear();

        var grassSegments = Random.Range(minLength, maxLength);
        _grassVectors =  new Vector3[grassSegments];

        for (var i = 0; i < grassSegments; i++)
        {
            var xPosition = i > 0
                ? _grassVectors[i-1].x + Random.Range(-2f, 2f)
                : _startXPosition;

            var yPosition = i > 0
                ? _grassVectors[i - 1].y + Random.Range(2f, 3f)
                : 0;

            var vector = new Vector3(xPosition, yPosition, 0);

            _grassVectors[i] = vector;

            _spriteShapeController.spline.InsertPointAt(i, vector);
            _spriteShapeController.spline.SetTangentMode(i, ShapeTangentMode.Continuous);

            _spriteShapeController.spline.SetHeight(i, startHeight - 0.1f * i);

            if (i <= 1)
                continue;

            var direction = _grassVectors[i - 1] - _grassVectors[i - 2];
            _spriteShapeController.spline.SetRightTangent(i-1, direction.normalized);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(_windChangeTimer <= 0)
            UpdateWindDirection();

        _windChangeTimer -= Time.deltaTime;

        // The grass should slowly move, to give the illusion of wind. It moves faster the higher it is.
        // each point should move a little bit to the left or right, keeping the original _grassVectors position in mind

        // MoveGrass();
    }

    private void MoveGrass()
    {
        for (var i = 1; i < _spriteShapeController.spline.GetPointCount(); i++)
        {
            var vector = _spriteShapeController.spline.GetPosition(i);

            var x = Mathf.Lerp(vector.x, _grassVectors[i].x + _windDirection.x, _windSpeed/10);
            var y = Mathf.Lerp(vector.y, _grassVectors[i].y +_windDirection.y, _windSpeed/10);

            _spriteShapeController.spline.SetPosition(i, new Vector3(x, y, 0));
        }
    }
}
