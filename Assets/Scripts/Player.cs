using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public enum PlayerAnimationState
{
    Idle,
    Walking,
    Death
}

public class Player : NetworkBehaviour
{
    public const string PlayerAnimationStateName = "State";
    [SerializeField] private Collider2D collider;
    [SerializeField] private PlayerHealth health;
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private PlayerRotation rotation;
    [SerializeField] private PlayerMagic magic;
    public static Player LocalInstance { get; private set; }
    public static List<Player> Players { get; private set; } = new List<Player>();
    public ulong OwnerId { get; private set; }

    public string GetName()
    {
        if (OwnerId == 0)
        {
            return "Host";
        }
        return "Client";
    }

    public override void OnNetworkSpawn()
    {
        OwnerId = OwnerClientId;
        Spawn();
        if (IsOwner)
        {
            health.OnPlayerDead += Health_OnPlayerDead;
            LocalInstance = this;
        }
        ArenaManager.Singleton.AddPlayer(this);
        Players.Add(this);
    }

    public override void OnNetworkDespawn()
    {
        Players.Remove(this);
    }

    public void Spawn()
    {
        transform.position = SpawnManager.Singleton.GetSpawnPointForPlayer((int)OwnerId);
        health.Resurrect();

        movement.enabled = true;
        rotation.enabled = true;
        magic.enabled = true;
        collider.enabled = true;
    }

    private void Health_OnPlayerDead(object sender, Player killer)
    {
        if (IsOwner)
        {
            DisableControl();
            ArenaManager.Singleton.PlayerDiedServerRpc(OwnerId, killer.OwnerId);
        }
    }

    public void DisableControl()
    {
        magic.ResetState();

        movement.enabled = false;
        rotation.enabled = false;
        magic.enabled = false;
        collider.enabled = false;
    }
}
