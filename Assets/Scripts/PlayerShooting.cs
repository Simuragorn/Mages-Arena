using Unity.Netcode;
using UnityEngine;

public class PlayerShooting : NetworkBehaviour
{
    [SerializeField] private Shell shellPrefab;
    [SerializeField] private Transform spawnPosition;
    [SerializeField] private float reloading = 0.5f;

    private float reloadingLeft = 0;

    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }
        reloadingLeft = Mathf.Max(0, reloadingLeft - Time.deltaTime);
        if (Input.GetButton("Fire1") && reloadingLeft <= 0)
        {
            ShootServerRpc(gameObject.GetComponent<NetworkObject>());
            reloadingLeft = reloading;
        }
    }
    [ServerRpc(RequireOwnership = false)]
    private void ShootServerRpc(NetworkObjectReference ownerReference)
    {        
        var shell = Instantiate(shellPrefab, spawnPosition.position, spawnPosition.rotation);
        var shellReference = shell.GetComponent<NetworkObject>();
        shellReference.Spawn(true);
        SetShellOwnerClientRpc(shellReference, ownerReference);
    }
    [ClientRpc]
    private void SetShellOwnerClientRpc(NetworkObjectReference shellReference, NetworkObjectReference ownerReference)
    {
        shellReference.TryGet(out NetworkObject shellObject);
        ownerReference.TryGet(out NetworkObject ownerObject);
        shellObject.GetComponent<Shell>().Launch(ownerObject.GetComponent<Player>());
    }
}
