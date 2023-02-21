using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] Transform[] spawnPoints;

    public static SpawnManager instance;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        DeactivateAllSpawnPoints();
    }

    public Transform GetSpawnPoint()
    {
        int spawnIndex = Random.Range(0, spawnPoints.Length);
        return spawnPoints[spawnIndex];
    }

    private void DeactivateAllSpawnPoints()
    {
        foreach (Transform spawn in spawnPoints)
        {
            spawn.gameObject.SetActive(false);
        }
    }

}
