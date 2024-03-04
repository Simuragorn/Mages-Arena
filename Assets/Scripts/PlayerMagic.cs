using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class PlayerMagic : NetworkBehaviour
{
    protected enum PlayerMagicState
    {
        None,
        Shoot,
        Shield
    }
    [SerializeField] private Transform shellSpawn;
    [SerializeField] private Transform shieldSpawn;
    private MagicType magicType;

    [SerializeField] private float stateDelay = 0.5f;
    [SerializeField] private float maxMana = 100;
    [SerializeField] private float manaRegeneration = 1.5f;

    public float ActualMana { get; private set; }

    private List<MagicType> magicTypes;
    private PlayerMagicState playerMagicState;
    private Shield latestShield;
    private float shootDelayLeft = 0;
    private float stateDelayLeft = 0;
    private bool shieldCreated = false;

    public Transform ShieldSpawn => shellSpawn;

    private void Awake()
    {
        ActualMana = maxMana;
        magicTypes = MagicTypesManager.Singleton.GetMagicTypes().ToList();
        magicType = magicTypes.First();
    }

    private void ChangeMana(float change)
    {
        ActualMana += change;
        ActualMana = Mathf.Max(ActualMana, 0);
        ActualMana = Mathf.Min(ActualMana, maxMana);
    }

    public void SetNewShield(Shield shield)
    {
        latestShield = shield;
    }

    public void ResetState()
    {
        ApplyNewState(PlayerMagicState.None);
    }

    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        HandleState();
        HandleManaRegeneration();
        HandleShoot();
        HandleShield();
    }

    private void HandleManaRegeneration()
    {
        if (playerMagicState == PlayerMagicState.None)
        {
            ChangeMana(manaRegeneration * Time.fixedDeltaTime);
        }
        if (playerMagicState == PlayerMagicState.Shield)
        {
            ChangeMana(-magicType.ShieldManaCost * Time.fixedDeltaTime);
        }
    }

    private void HandleState()
    {
        stateDelayLeft = Mathf.Max(0, stateDelayLeft - Time.deltaTime);
        PlayerMagicState newState = PlayerMagicState.None;
        if (Input.GetButton("Fire1"))
        {
            newState = PlayerMagicState.Shoot;
        }
        if (Input.GetButton("Fire2"))
        {
            newState = PlayerMagicState.Shield;
        }
        ApplyNewState(newState);
    }

    private void ApplyNewState(PlayerMagicState newState)
    {
        if (newState == PlayerMagicState.None ||
            (newState != playerMagicState && stateDelayLeft <= 0))
        {
            if (latestShield != null)
            {
                DeleteShieldServerRpc(latestShield.GetComponent<NetworkObject>());
                latestShield = null;
                shieldCreated = false;
            }
            playerMagicState = newState;
            stateDelayLeft = stateDelay;
        }
    }

    private void HandleShield()
    {
        if (playerMagicState == PlayerMagicState.Shield)
        {
            TryUseShield();
        }
    }

    private void TryUseShield()
    {
        if (shieldCreated && ActualMana <= 0)
        {
            ResetState();
            return;
        }
        if (!shieldCreated)
        {
            shieldCreated = true;
            ShieldServerRpc(gameObject.GetComponent<NetworkObject>());
        }
    }

    private void HandleShoot()
    {
        shootDelayLeft = Mathf.Max(0, shootDelayLeft - Time.deltaTime);
        if (playerMagicState == PlayerMagicState.Shoot)
        {
            TryShoot();
        }
    }

    private void TryShoot()
    {
        if (shootDelayLeft <= 0 && ActualMana >= magicType.ShootManaCost)
        {
            playerMagicState = PlayerMagicState.Shoot;
            ShootServerRpc(gameObject.GetComponent<NetworkObject>());
            shootDelayLeft = magicType.ShootDelay;
            ChangeMana(-magicType.ShootManaCost);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void DeleteShieldServerRpc(NetworkObjectReference ownerReference)
    {
        ownerReference.TryGet(out NetworkObject shieldObject);
        Destroy(shieldObject.gameObject);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ShieldServerRpc(NetworkObjectReference ownerReference)
    {
        var shield = Instantiate(magicType.ShieldPrefab, shieldSpawn.position, shieldSpawn.rotation);
        var shieldReference = shield.GetComponent<NetworkObject>();
        shieldReference.Spawn(true);
        SetShieldOwnerClientRpc(shieldReference, ownerReference);
    }

    [ClientRpc]
    private void SetShieldOwnerClientRpc(NetworkObjectReference shieldReference, NetworkObjectReference ownerReference)
    {
        shieldReference.TryGet(out NetworkObject shieldOject);
        ownerReference.TryGet(out NetworkObject ownerObject);
        var playerMagic = ownerObject.gameObject.GetComponent<PlayerMagic>();
        Transform shieldSpawn = playerMagic.shieldSpawn;
        var shield = shieldOject.GetComponent<Shield>();
        shield.Launch(shieldSpawn);
        playerMagic.SetNewShield(shield);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ShootServerRpc(NetworkObjectReference ownerReference)
    {
        var shell = Instantiate(magicType.ShellPrefab, shellSpawn.position, shellSpawn.rotation);
        var shellReference = shell.GetComponent<NetworkObject>();
        shellReference.Spawn(true);
        SetShellOwnerClientRpc(shellReference, ownerReference);
    }
    [ClientRpc]
    private void SetShellOwnerClientRpc(NetworkObjectReference shellReference, NetworkObjectReference ownerReference)
    {
        shellReference.TryGet(out NetworkObject shellObject);
        ownerReference.TryGet(out NetworkObject ownerObject);
        shellObject.GetComponent<Shell>().Launch(ownerObject.GetComponent<Player>(), magicType.Type);
    }
}
