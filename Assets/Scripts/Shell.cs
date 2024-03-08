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
    private GameObject prefab;
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
        PrepareForLaunch(magicTypeEnum);
        ownerPlayer = sendingPlayer;
        isLaunched = true;
        loopingVFX.gameObject.SetActive(true);
        if (IsServer)
        {
            rigidbody.AddForce(MagicType.ShootImpulse * transform.up, ForceMode2D.Impulse);
        }
        if (IsActivatable)
        {
            sendingPlayer.GetComponent<PlayerMagic>().AddActivatableShell(this);
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
            NetworkObjectPool.Singleton.ReturnNetworkObject(NetworkObject, prefab, 2f);
        }
    }

    public void ImpactEffects()
    {
        if (IsServer)
        {
            var impactVFXObject = NetworkObjectPool.Singleton.GetNetworkObject(impactVFX.gameObject, transform.position, transform.rotation);
            if (!impactVFXObject.IsSpawned)
            {
                impactVFXObject.GetComponent<NetworkObject>().Spawn();
            }
            NetworkObjectPool.Singleton.ReturnNetworkObject(impactVFXObject.GetComponent<NetworkObject>(), impactVFX.gameObject, 2f);
        }
    }
    public void DestroyEffects()
    {
        if (IsServer)
        {
            var destroyVFXObject = NetworkObjectPool.Singleton.GetNetworkObject(destroyVFX.gameObject, transform.position, transform.rotation);
            if (!destroyVFXObject.IsSpawned)
            {
                destroyVFXObject.GetComponent<NetworkObject>().Spawn();
            }
            NetworkObjectPool.Singleton.ReturnNetworkObject(destroyVFXObject.GetComponent<NetworkObject>(), destroyVFX.gameObject, 2f);
        }
    }
    private void PrepareForLaunch(MagicTypeEnum magicTypeEnum)
    {
        collider.isTrigger = false;
        rigidbody.velocity = Vector2.zero;
        rigidbody.simulated = true;
        var loopingParticles = loopingVFX.GetComponentsInChildren<ParticleSystem>().ToList();
        loopingParticles.Add(loopingVFX);
        loopingParticles.ForEach(particle =>
        {
            var main = particle.main;
            main.loop = true;
        });

        MagicType = MagicTypesManager.Singleton.GetMagicTypes().First(m => m.Type == magicTypeEnum);
        prefab = MagicType.ShellPrefab.gameObject;

        transform.rotation = transform.rotation * Quaternion.AngleAxis(Random.Range(-MagicType.ShootSpreadAngle / 2, MagicType.ShootSpreadAngle / 2), Vector3.forward);
        IsActivatable = MagicType.IsActivatableShoot;
        gameObject.layer = LayerMask.NameToLayer(MagicType.GetLayerName(magicTypeEnum, MagicEquipmentType.Shell));
        ricochetCountLeft = MagicType.RicochetCount;
        rigidbody.sharedMaterial = MagicType.ShellPhysicsMaterial;
        isActivated = false;
        if (IsActivatable)
        {
            activatedLoopingVFX.gameObject.SetActive(false);
        }
        loopingVFX.gameObject.SetActive(false);
        rigidbody.useFullKinematicContacts = true;
    }
}
