using System;
using FishNet.Object;
using UnityEngine;

public class PlayerHealth : NetworkBehaviour
{
    [SerializeField] private ParticleSystem deathVFX;
    [SerializeField] private ParticleSystem spawnVFX;
    [SerializeField] private Animator animator;
    private bool isDead = false;
    private Player lastDamageDealer;
    private Player player;
    public event EventHandler<Player> OnPlayerDead;
    public bool IsDead => isDead;

    private void Awake()
    {
        player = GetComponent<Player>();
    }
    public void Resurrect()
    {
        player.DisableControl();
        StartResurrect();
    }

    private void StartResurrect()
    {
        isDead = false;
        spawnVFX.gameObject.SetActive(true);
        animator.SetInteger(Player.PlayerAnimationStateName, (int)PlayerAnimationState.Spawn);
        spawnVFX.gameObject.SetActive(true);
        Invoke(nameof(CommitResurrect), 1.5f);
    }
    public void CommitResurrect()
    {
        player.EnableControl();
    }

    public void GetDamage(Player damageDealer)
    {
        int playerIndex = Player.Players.IndexOf(GetComponent<Player>());
        int damageDealerIndex = Player.Players.IndexOf(damageDealer);
        GetDamageServerRpc(playerIndex, damageDealerIndex);
    }

    [ServerRpc(RequireOwnership = false)]
    protected void GetDamageServerRpc(int playerIndex, int damageDealerIndex)
    {
        GetDamageClientRpc(playerIndex, damageDealerIndex);
    }
    [ObserversRpc]
    protected void GetDamageClientRpc(int playerIndex, int damageDealerIndex)
    {
        var player = Player.Players[playerIndex];
        var damageDealer = Player.Players[damageDealerIndex];
        player.GetComponent<PlayerHealth>().StartDeath(damageDealer);
    }
    public void StartDeath(Player damageDealer)
    {
        player.DisableControl();
        isDead = true;
        animator.SetInteger(Player.PlayerAnimationStateName, (int)PlayerAnimationState.Death);
        deathVFX.gameObject.SetActive(true);
        lastDamageDealer = damageDealer;

        Invoke(nameof(CommitDeath), 1.5f);
    }
    public void CommitDeath()
    {
        OnPlayerDead?.Invoke(this, lastDamageDealer);
    }
}
