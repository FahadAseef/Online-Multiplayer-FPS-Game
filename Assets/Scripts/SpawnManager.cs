using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager instance;
    public Transform[] SpawnPoints;

    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        foreach(Transform spawn in SpawnPoints)
        {
            spawn.gameObject.SetActive(false);
        }
    }
    public Transform GetSpawnPoints()
    {
        return SpawnPoints[Random.Range(0,SpawnPoints.Length)];
    }
}
