using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class Shell : NetworkBehaviour
{
    [SerializeField] private ParticleSystem loopingVFX;
    [SerializeField] private ParticleSystem explosionVFX;

    private MagicType magicType;

    private bool isLaunched = false;
    private Player ownerPlayer;

    public void Launch(Player sendingPlayer, MagicTypeEnum magicTypeEnum)
    {
        ownerPlayer = sendingPlayer;
        isLaunched = true;
        magicType = MagicTypesManager.Singleton.GetMagicTypes().First(m => m.Type == magicTypeEnum);
    }


    void Update()
    {
        if (!isLaunched)
        {
            return;
        }
        transform.Translate(magicType.ShootSpeed * Time.deltaTime * Vector2.up);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isLaunched)
        {
            return;
        }
        if (collision.CompareTag("Player") && ownerPlayer != collision.gameObject.GetComponent<Player>())
        {
            collision.GetComponent<PlayerHealth>().GetDamage(ownerPlayer);
            DestroyShell();
        }
        if (collision.CompareTag("Target"))
        {
            DestroyShell();
        }
    }

    private void DestroyShell()
    {
        isLaunched = false;

        var loopingParticles = loopingVFX.GetComponentsInChildren<ParticleSystem>().ToList();
        loopingParticles.Add(loopingVFX);
        loopingParticles.ForEach(particle =>
        {
            var main = particle.main;
            main.loop = false;
        });

        explosionVFX.gameObject.SetActive(true);
        if (IsServer)
        {
            Destroy(gameObject, 3f);
        }
        //DestroyShellServerRpc(GetComponent<NetworkObject>());
    }

    [ServerRpc(RequireOwnership = false)]
    private void DestroyShellServerRpc(NetworkObjectReference shellReference)
    {
        shellReference.TryGet(out NetworkObject shell);
        Destroy(shell.gameObject, 3f);
    }
}
