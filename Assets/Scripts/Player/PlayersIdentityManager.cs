using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;

public class PlayersIdentityManager : NetworkSingleton<PlayersIdentityManager>
{
    [SerializeField] private List<PlayerIdentity> playerIdentities;
    public IReadOnlyList<PlayerIdentity> GetPlayerIdentities()
    {
        return playerIdentities;
    }
}
