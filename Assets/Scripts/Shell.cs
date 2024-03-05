using System.Linq;
using Unity.Netcode;
using UnityEngine;

public enum HitType
{
    Impact,
    Destroy
}
public class Shell : NetworkBehaviour
{
    [SerializeField] private ParticleSystem loopingVFX;
    [SerializeField] private ParticleSystem destroyVFX;
    [SerializeField] private ParticleSystem impactVFX;
    [SerializeField] private Rigidbody2D rigidbody;
    [SerializeField] private Collider2D collider;

    public MagicType MagicType { get; private set; }

    private bool isLaunched = false;
    private bool isLeftPlayer = false;
    private int ricochetCountLeft;
    private Player ownerPlayer;

    public void Launch(Player sendingPlayer, MagicTypeEnum magicTypeEnum)
    {
        collider.isTrigger = true;
        ownerPlayer = sendingPlayer;
        MagicType = MagicTypesManager.Singleton.GetMagicTypes().First(m => m.Type == magicTypeEnum);
        gameObject.layer = LayerMask.NameToLayer(MagicType.GetLayerName(magicTypeEnum, MagicEquipmentType.Shell));
        ricochetCountLeft = MagicType.RicochetCount;
        rigidbody.sharedMaterial = MagicType.ShellPhysicsMaterial;
        rigidbody.AddForce(MagicType.ShootImpulse * transform.up, ForceMode2D.Impulse);

        isLaunched = true;
    }


    void Update()
    {
        if (!IsOwner)
        {
            return;
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        var collisionObject = collision.gameObject;
        if (!isLaunched)
        {
            return;
        }
        ricochetCountLeft--;
        HitType hitType = HitType.Impact;
        if (collisionObject.CompareTag("Player"))
        {
            if (isLeftPlayer)
            {
                collisionObject.GetComponent<PlayerHealth>().GetDamage(ownerPlayer);
                hitType = HitType.Destroy;
            }
        }
        if (collisionObject.CompareTag("Target"))
        {
            Shield shield = collisionObject.GetComponent<Shield>();
            if (shield != null && shield.MagicTypeValue.Type == MagicType.Type)
            {
                hitType = HitType.Destroy;
            }
            else if (ricochetCountLeft <= 0)
            {
                hitType = HitType.Destroy;
            }
        }

        if (hitType == HitType.Destroy)
        {
            DestroyShell();
        }
        else if (hitType == HitType.Impact)
        {
            ImpactEffects();
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Player player = collision.GetComponent<Player>();
            if (player == ownerPlayer)
            {
                isLeftPlayer = true;
                collider.isTrigger = false;
            }
        }
    }

    private void DestroyShell()
    {
        isLaunched = false;
        isLeftPlayer = false;
        rigidbody.velocity = Vector2.zero;
        rigidbody.isKinematic = true;
        var loopingParticles = loopingVFX.GetComponentsInChildren<ParticleSystem>().ToList();
        loopingParticles.Add(loopingVFX);
        loopingParticles.ForEach(particle =>
        {
            var main = particle.main;
            main.loop = false;
        });
        DestroyEffects();
        if (IsServer)
        {
            Destroy(gameObject, 3f);
        }
    }

    public void ImpactEffects()
    {
        if (IsServer)
        {
            var impactVFXObject = Instantiate(impactVFX, transform.position, transform.rotation);
            impactVFXObject.GetComponent<NetworkObject>().Spawn();
        }
    }
    public void DestroyEffects()
    {
        if (IsServer)
        {
            var destroyVFXObject = Instantiate(destroyVFX, transform.position, transform.rotation);
            destroyVFXObject.GetComponent<NetworkObject>().Spawn();
        }
    }
}
