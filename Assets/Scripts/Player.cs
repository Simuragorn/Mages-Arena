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

    private void Start()
    {
        transform.position = SpawnManager.Singleton.GetSpawnPointForPlayer((int)OwnerClientId);
        health.OnPlayerDead += Health_OnPlayerDead;
    }

    private void Health_OnPlayerDead(object sender, System.EventArgs e)
    {
        movement.enabled = false;
        rotation.enabled = false;
        shooting.enabled = false;
        collider.enabled = false;

        Destroy(gameObject, 2f);
    }
}
