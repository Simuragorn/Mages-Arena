using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public enum PlayerAnimationState
{
    Idle,
    Walking,
    Death,
    Spawn,
}

public class Player : NetworkBehaviour
{
    public const string PlayerAnimationStateName = "State";
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private Animator animator;
    [SerializeField] private Collider2D collider;
    [SerializeField] private PlayerHealth health;
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private PlayerRotation rotation;
    [SerializeField] private PlayerMagic magic;
    private PlayerIdentity identity;
    public static Player LocalInstance { get; private set; }
    public static List<Player> Players { get; private set; } = new List<Player>();

    public string GetName()
    {
        if (identity != null)
        {
            return identity.Name;
        }
        return OwnerClientId == 0 ? "Host" : "Client";
    }

    private void Awake()
    {
        sprite.enabled = false;
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            health.OnPlayerDead += Health_OnPlayerDead;
            LocalInstance = this;
        }
        Players.Add(this);
        var identities = PlayersIdentityManager.Singleton.GetPlayerIdentities();
        identity = identities.FirstOrDefault(i => i.Index == Players.Count - 1);
        ArenaManager.Singleton.AddPlayer(this);
        Spawn();
    }

    public override void OnNetworkDespawn()
    {
        ArenaManager.Singleton.RemovePlayer(this);
        Players.Remove(this);
    }

    public void Spawn()
    {
        transform.position = SpawnManager.Singleton.GetSpawnPointForPlayer(identity.Index);
        animator.runtimeAnimatorController = identity.Animator;
        sprite.sprite = identity.Sprite;
        sprite.enabled = true;
        magic.Refresh();
        health.Resurrect();
    }

    private void Health_OnPlayerDead(object sender, Player killer)
    {
        if (IsOwner)
        {
            ArenaManager.Singleton.PlayerDiedServerRpc(OwnerClientId, killer.OwnerClientId);
        }
    }

    public void EnableControl()
    {
        movement.enabled = true;
        rotation.enabled = true;
        magic.enabled = true;
        collider.enabled = true;
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
