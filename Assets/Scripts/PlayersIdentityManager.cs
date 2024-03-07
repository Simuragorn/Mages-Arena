using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayersIdentityManager : NetworkBehaviour
{
    [SerializeField] private List<PlayerIdentity> playerIdentities;
    public static PlayersIdentityManager Singleton;
    private void Awake()
    {
        var instances = FindObjectsByType<PlayersIdentityManager>(FindObjectsSortMode.None);
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
    public IReadOnlyList<PlayerIdentity> GetPlayerIdentities()
    {
        return playerIdentities;
    }
}
