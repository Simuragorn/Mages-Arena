using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private ParticleSystem deathVFX;
    [SerializeField] private Animator animator;
    private bool isDead = false;
    public event EventHandler OnPlayerDead;
    public bool IsDead => isDead;
    public void GetDamage()
    {
        isDead = true;
        OnPlayerDead?.Invoke(this, EventArgs.Empty);
        animator.SetInteger(Player.PlayerAnimationStateName, (int)PlayerAnimationState.Death);
        deathVFX.gameObject.SetActive(true);
    }
}
