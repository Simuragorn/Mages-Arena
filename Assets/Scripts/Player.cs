using System;
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
    [SerializeField] private PlayerShooting shooting;
    public static Player LocalInstance { get; private set; }
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
    }

    public void Spawn()
    {
        transform.position = SpawnManager.Singleton.GetSpawnPointForPlayer((int)OwnerId);
        health.Resurrect();

        movement.enabled = true;
        rotation.enabled = true;
        shooting.enabled = true;
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
        movement.enabled = false;
        rotation.enabled = false;
        shooting.enabled = false;
        collider.enabled = false;
    }
}
