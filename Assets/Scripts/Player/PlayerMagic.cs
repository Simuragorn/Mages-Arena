using System.Collections.Generic;
using System.Linq;
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
    [SerializeField] private float minManaForShieldSpawn = 2f;

    public float ActualMana { get; private set; }
    public float MaxMana => maxMana;

    private List<MagicType> magicTypes;
    private PlayerMagicState playerMagicState;
    public Shield LatestShield;
    private float shootDelayLeft = 0;
    private float stateDelayLeft = 0;

    private List<Shell> activatableShells = new List<Shell>();

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

    public void AddActivatableShell(Shell shell)
    {
        activatableShells.Add(shell);
    }

    public void RemoveActivatableShell(Shell shell)
    {
        activatableShells.Remove(shell);
    }

    private void HandleShellsActivation()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            activatableShells = activatableShells.Where(s => s != null).ToList();
            if (activatableShells.Any())
            {
                ActivateShellsRpc(OwnerClientId);
            }
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ActivateShellsRpc(ulong ownerClientId)
    {
        var playerMagic = Player.Players.First(p => p.OwnerClientId == ownerClientId).GetComponent<PlayerMagic>();
        playerMagic.activatableShells.ForEach(s => s.Activate());
        playerMagic.activatableShells.Clear();
    }

    public void Refresh()
    {
        magicType = magicTypes.First();
        ActualMana = maxMana;
    }

    public void SetNewShield(Shield shield)
    {
        LatestShield = shield;
        shield.Activate();
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
        HandleShellsActivation();
        HandleMagicTypeChange();
        HandleState();
        HandleShoot();
        HandleShield();
    }

    private void FixedUpdate()
    {
        if (!IsOwner)
        {
            return;
        }
        HandleManaRegeneration();
    }

    private void HandleMagicTypeChange()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            int magicTypeIndex = magicTypes.IndexOf(magicType);
            magicTypeIndex++;
            if (magicTypeIndex == magicTypes.Count)
            {
                magicTypeIndex = 0;
            }
            magicType = magicTypes[magicTypeIndex];

        }
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
        if (ActualMana <= 0)
        {
            ResetState();
            return;
        }

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
            if (LatestShield != null)
            {
                DeleteShieldRpc(GetComponent<NetworkObject>());
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
        if (LatestShield == null && ActualMana >= minManaForShieldSpawn)
        {
            SetShieldOwnerRpc(GetComponent<NetworkObject>(), magicType.Type);
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
            ShootServerRpc(gameObject.GetComponent<NetworkObject>(), magicType.Type);
            shootDelayLeft = magicType.ShootDelay;
            ChangeMana(-magicType.ShootManaCost);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void DeleteShieldRpc(NetworkObjectReference ownerReference)
    {
        ownerReference.TryGet(out NetworkObject ownerObject);
        var playerMagic = ownerObject.GetComponent<PlayerMagic>();
        if (playerMagic.LatestShield != null)
        {
            playerMagic.LatestShield.Deactivate();
            playerMagic.LatestShield = null;
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SetShieldOwnerRpc(NetworkObjectReference ownerReference, MagicTypeEnum magicTypeEnum)
    {
        Shield shield = shieldSpawn.GetComponentsInChildren<Shield>(true).First(s => s.MagicTypeEnum == magicTypeEnum);

        ownerReference.TryGet(out NetworkObject ownerObject);
        var playerMagic = ownerObject.gameObject.GetComponent<PlayerMagic>();
        playerMagic.SetNewShield(shield);
    }

    [Rpc(SendTo.Server)]
    private void ShootServerRpc(NetworkObjectReference ownerReference, MagicTypeEnum magicTypeEnum)
    {
        var magicTypes = MagicTypesManager.Singleton.GetMagicTypes();
        var actualMagicType = magicTypes.First(m => m.Type == magicTypeEnum);
        var shell = NetworkObjectPool.Singleton.GetNetworkObject(actualMagicType.ShellPrefab.gameObject, shellSpawn.position, shellSpawn.rotation).gameObject;
        var shellReference = shell.GetComponent<NetworkObject>();
        if (!shellReference.IsSpawned)
        {
            shellReference.Spawn(true);
        }
        SetShellOwnerRpc(shellReference, ownerReference, magicTypeEnum);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SetShellOwnerRpc(NetworkObjectReference shellReference, NetworkObjectReference ownerReference, MagicTypeEnum magicTypeEnum)
    {
        var magicTypes = MagicTypesManager.Singleton.GetMagicTypes();
        var actualMagicType = magicTypes.First(m => m.Type == magicTypeEnum);

        shellReference.TryGet(out NetworkObject shellObject);
        ownerReference.TryGet(out NetworkObject ownerObject);
        shellObject.GetComponent<Shell>().Launch(ownerObject.GetComponent<Player>(), actualMagicType.Type);
    }
}
