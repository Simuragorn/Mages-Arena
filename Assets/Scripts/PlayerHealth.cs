using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            GetDamage(GetComponent<Player>());
        }
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
    }
    public void CommitResurrect()
    {
        player.EnableControl();
    }
    public void GetDamage(Player damageDealer)
    {
        StartDeath(damageDealer);
    }
    private void StartDeath(Player damageDealer)
    {
        player.DisableControl();
        isDead = true;
        animator.SetInteger(Player.PlayerAnimationStateName, (int)PlayerAnimationState.Death);
        deathVFX.gameObject.SetActive(true);
        lastDamageDealer = damageDealer;
    }
    public void CommitDeath()
    {
        OnPlayerDead?.Invoke(this, lastDamageDealer);
    }
}
