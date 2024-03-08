using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayersIdentityManager : NetworkSingleton<PlayersIdentityManager>
{
    [SerializeField] private List<PlayerIdentity> playerIdentities;
    public IReadOnlyList<PlayerIdentity> GetPlayerIdentities()
    {
        return playerIdentities;
    }
}
