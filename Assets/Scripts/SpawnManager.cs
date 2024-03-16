using System;
using System.Collections.Generic;
using System.Linq;
using FishNet.Object;
using UnityEngine;

public class SpawnManager : NetworkSingleton<SpawnManager>
{
    [SerializeField] private List<Transform> playerSpawnPoints;
    public Vector2 GetSpawnPointForPlayer(int playerId)
    {
        if (playerSpawnPoints.Count <= playerId)
        {
            return playerSpawnPoints.First().position;
        }
        return playerSpawnPoints[playerId].position;
    }
}
