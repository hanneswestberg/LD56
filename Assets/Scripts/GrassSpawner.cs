using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassSpawner : MonoBehaviour
{
    [SerializeField] private GameObject grassStrawPrefab;
    [SerializeField] private int grassStrawCount;
    [SerializeField] private float grassStrawSpacing;

    public bool spawnGrassOnStart = false;

    public void Start()
    {
        if (spawnGrassOnStart)
        {
            SpawnGrass();
        }
    }

    public void SpawnGrass()
    {
        SpawnGrass(grassStrawCount);
    }

    private void SpawnGrass(int count)
    {
        var startX = - (grassStrawCount * grassStrawSpacing) / 2;

        for (var i = 0; i < count; i++)
        {
            var grassStraw = Instantiate(grassStrawPrefab, new Vector3(startX + i * grassStrawSpacing, 0, 0), Quaternion.identity);
            grassStraw.transform.SetParent(transform);
        }
    }
}
