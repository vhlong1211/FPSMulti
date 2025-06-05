using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : SingletonMonoBehaviour<SpawnManager>
{
    public List<Transform> spawnPoints;

    public Transform GetRandomPoint()
    {
        return spawnPoints[Random.Range(0, spawnPoints.Count)];
    }
}
