using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private ParticleSystem deathVFX;
    [SerializeField] private Animator animator;
    private bool isDead = false;
    public event EventHandler<Player> OnPlayerDead;
    public bool IsDead => isDead;
    public Player lastDamageDealer;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            GetDamage(GetComponent<Player>());
        }
    }
    public void Resurrect()
    {
        isDead = false;
        deathVFX.gameObject.SetActive(false);
    }
    public void GetDamage(Player damageDealer)
    {
        StartDeath(damageDealer);
    }
    private void StartDeath(Player damageDealer)
    {
        Player.LocalInstance.DisableControl();
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
