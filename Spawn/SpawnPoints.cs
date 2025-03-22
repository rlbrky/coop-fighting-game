using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoints : MonoBehaviour
{
    public static SpawnPoints Instance;

    [SerializeField] private List<Transform> spawnPoints_TeamA = new List<Transform>();
    [SerializeField] private List<Transform> spawnPoints_TeamB = new List<Transform>();
    
    private void Awake()
    {
        Instance = this;
    }

    public Transform GetRandomSpawnPoint_TeamA()
    {
        int index = Random.Range(0, spawnPoints_TeamA.Count);
        return spawnPoints_TeamA[index];
    }

    public Transform GetRandomSpawnPoint_TeamB()
    {
        int index = Random.Range(0, spawnPoints_TeamB.Count);
        return spawnPoints_TeamB[index];
    }
}