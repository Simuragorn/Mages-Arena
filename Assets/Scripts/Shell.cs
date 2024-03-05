using System.Collections;
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
    private int ricochetCountLeft;
    private Player ownerPlayer;

    public void Launch(Player sendingPlayer, MagicTypeEnum magicTypeEnum)
    {
        collider.isTrigger = false;
        ownerPlayer = sendingPlayer;
        MagicType = MagicTypesManager.Singleton.GetMagicTypes().First(m => m.Type == magicTypeEnum);
        gameObject.layer = LayerMask.NameToLayer(MagicType.GetLayerName(magicTypeEnum, MagicEquipmentType.Shell));
        ricochetCountLeft = MagicType.RicochetCount;
        rigidbody.sharedMaterial = MagicType.ShellPhysicsMaterial;
        isLaunched = true;

        if (IsOwner)
        {
            rigidbody.AddForce(MagicType.ShootImpulse * transform.up, ForceMode2D.Impulse);
        }
        rigidbody.useFullKinematicContacts = true;

        StartCoroutine(DelayForCollisionWithOwner());
    }

    private IEnumerator DelayForCollisionWithOwner()
    {
        Physics2D.IgnoreCollision(ownerPlayer.GetComponent<Collider2D>(), collider);
        yield return new WaitForSeconds(0.5f);
        Physics2D.IgnoreCollision(ownerPlayer.GetComponent<Collider2D>(), collider, false);
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
        if (!IsOwner)
        {
            return;
        }
        var collisionObject = collision.gameObject;
        if (!isLaunched)
        {
            return;
        }
        ricochetCountLeft--;
        HitType hitType = HitType.Impact;

        if (collisionObject.CompareTag("Player"))
        {
            collisionObject.GetComponent<PlayerHealth>().GetDamage(ownerPlayer);
            hitType = HitType.Destroy;
        }

        if (ricochetCountLeft <= 0)
        {
            hitType = HitType.Destroy;
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

    private void DestroyShell()
    {
        isLaunched = false;
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
            Destroy(gameObject);
        }
    }

    public void ImpactEffects()
    {
        if (IsServer)
        {
            var impactVFXObject = Instantiate(impactVFX, transform.position, transform.rotation);
            impactVFXObject.GetComponent<NetworkObject>().Spawn();

            Destroy(impactVFXObject.gameObject, 2f);
        }
    }
    public void DestroyEffects()
    {
        if (IsServer)
        {
            var destroyVFXObject = Instantiate(destroyVFX, transform.position, transform.rotation);
            destroyVFXObject.GetComponent<NetworkObject>().Spawn();

            Destroy(destroyVFXObject.gameObject, 2f);
        }
    }
}
