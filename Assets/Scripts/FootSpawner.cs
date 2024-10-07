using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class FootSpawner : MonoBehaviour
{
    public static FootSpawner Instance;

    public List<GameObject> foots;

    [SerializeField] private GameObject _footPrefab;
    [SerializeField] private float _spawnRate = 8f;

    private float _spawnTimer = 0;
    private bool _isSpawning;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void StartSpawning()
    {
        _isSpawning = true;
    }

    void Update()
    {
        _spawnTimer -= Time.deltaTime;

        if (_isSpawning && _spawnTimer <= 0)
        {
            // make sure we dont spawn too close x position

            float xPosition;
            var tries = 0;
            do
            {
                xPosition = Random.Range(-20f, 20f);
                tries++;
            } while (foots.Exists(foot => Mathf.Abs(foot.transform.position.x - xPosition) < 6f) && tries < 10);

            if (tries < 10)
            {
                var gameObject = Instantiate(_footPrefab, new Vector3(xPosition, 0, 0), Quaternion.identity);

                foots.Add(gameObject);
            }

            _spawnTimer = _spawnRate;
        }
    }
}
