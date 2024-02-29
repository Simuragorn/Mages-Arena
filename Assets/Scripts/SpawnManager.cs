using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] private List<Transform> playerSpawnPoints;
    public static SpawnManager Singleton { get; private set; }

    private void Awake()
    {
        if (Singleton != null)
        {
            Destroy(gameObject);
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
