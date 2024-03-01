using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class SpawnManager : NetworkBehaviour
{
    [SerializeField] private List<Transform> playerSpawnPoints;
    public static SpawnManager Singleton { get; private set; }

    private void Awake()
    {
        var instances = FindObjectsByType<SpawnManager>(FindObjectsSortMode.None);
        if (instances.Length > 1)
        {
            Singleton = instances[0];
            if (IsServer)
            {
                Destroy(gameObject);
            }
            return;
        }
        Singleton = this;
    }
    public Vector2 GetSpawnPointForPlayer(int playerId)
    {
        if (playerSpawnPoints.Count <= playerId)
        {
            return playerSpawnPoints.First().position;
        }
        return playerSpawnPoints[playerId].position;
    }
}
