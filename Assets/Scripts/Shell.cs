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
    [SerializeField] private ParticleSystem activatedLoopingVFX;
    [SerializeField] private ParticleSystem destroyVFX;
    [SerializeField] private ParticleSystem impactVFX;
    [SerializeField] private Rigidbody2D rigidbody;
    [SerializeField] private Collider2D collider;

    public MagicType MagicType { get; private set; }

    private bool isLaunched = false;
    private int ricochetCountLeft;
    private Player ownerPlayer;
    private bool IsActivatable;
    private bool isActivated;

    public void Launch(Player sendingPlayer, MagicTypeEnum magicTypeEnum)
    {
        collider.isTrigger = false;
        ownerPlayer = sendingPlayer;
        MagicType = MagicTypesManager.Instance.GetMagicTypes().First(m => m.Type == magicTypeEnum);
        transform.rotation = transform.rotation * Quaternion.AngleAxis(Random.Range(-MagicType.ShootSpreadAngle / 2, MagicType.ShootSpreadAngle / 2), Vector3.forward);
        IsActivatable = MagicType.IsActivatableShoot;
        gameObject.layer = LayerMask.NameToLayer(MagicType.GetLayerName(magicTypeEnum, MagicEquipmentType.Shell));
        ricochetCountLeft = MagicType.RicochetCount;
        rigidbody.sharedMaterial = MagicType.ShellPhysicsMaterial;
        isLaunched = true;

        if (IsServer)
        {
            rigidbody.AddForce(MagicType.ShootImpulse * transform.up, ForceMode2D.Impulse);
        }
        rigidbody.useFullKinematicContacts = true;
        if (IsActivatable)
        {
            sendingPlayer.GetComponent<PlayerMagic>().AddActivatableShell(this);
            activatedLoopingVFX.gameObject.SetActive(false);
        }

        StartCoroutine(DelayForCollisionWithOwner());
    }

    public void Activate()
    {
        if (isActivated || !isLaunched)
        {
            return;
        }
        isActivated = true;

        Transform target = FindClosestTarget();
        if (target != null)
        {
            rigidbody.velocity = Vector3.zero;
            Vector2 direction = (target.position - transform.position).normalized;
            if (IsServer)
            {
                rigidbody.AddForce(MagicType.ShootImpulse * direction, ForceMode2D.Impulse);
            }
        }

        if (loopingVFX != null && activatedLoopingVFX != null)
        {
            loopingVFX.gameObject.SetActive(false);
            activatedLoopingVFX.gameObject.SetActive(true);
        }
    }

    private IEnumerator DelayForCollisionWithOwner()
    {
        Physics2D.IgnoreCollision(ownerPlayer.GetComponent<Collider2D>(), collider);
        yield return new WaitForSeconds(0.5f);
        Physics2D.IgnoreCollision(ownerPlayer.GetComponent<Collider2D>(), collider, false);
    }

    public Transform FindClosestTarget()
    {
        if (Player.Players == null)
        {
            return null;
        }
        var distances = Player.Players.Where(p => p != null && p != ownerPlayer).Select(player => new
        {
            Player = player,
            Distance = Vector2.Distance(player.transform.position, transform.position)
        });

        var closestPlayer = distances.OrderBy(d => d.Distance).FirstOrDefault();
        if (closestPlayer == null)
        {
            return null;
        }
        return closestPlayer.Player.transform;
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsServer)
        {
            return;
        }
        if (!isLaunched)
        {
            return;
        }
        var collisionObject = collision.gameObject;
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

        HandleCollisionRpc(hitType);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void HandleCollisionRpc(HitType hitType)
    {
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
        rigidbody.simulated = false;
        var loopingParticles = loopingVFX.GetComponentsInChildren<ParticleSystem>().ToList();
        loopingParticles.Add(loopingVFX);
        loopingParticles.ForEach(particle =>
        {
            var main = particle.main;
            main.loop = false;
        });
        DestroyEffects();
        if (IsActivatable)
        {
            ownerPlayer.GetComponent<PlayerMagic>().RemoveActivatableShell(this);
        }
        if (IsServer)
        {
            Destroy(gameObject, 2f);
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
