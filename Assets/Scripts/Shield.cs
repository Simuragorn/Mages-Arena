using System.Linq;
using UnityEngine;

public class Shield : MonoBehaviour
{
    public MagicType MagicTypeValue { get; private set; }
    [SerializeField] private Collider2D collider;
    [SerializeField] private MagicTypeEnum magicTypeEnum;
    [SerializeField] private ParticleSystem shieldVFX;
    public MagicTypeEnum MagicTypeEnum => magicTypeEnum;
    public void Awake()
    {
        MagicTypeValue = MagicTypesManager.Singleton.GetMagicTypes().First(m => m.Type == magicTypeEnum);
        gameObject.layer = LayerMask.NameToLayer(MagicType.GetLayerName(magicTypeEnum, MagicEquipmentType.Shield));
        Deactivate();
    }

    public void Activate()
    {
        collider.enabled = true;
        shieldVFX.gameObject.SetActive(true);
    }

    public void Deactivate()
    {
        collider.enabled = false;
        shieldVFX.gameObject.SetActive(false);
    }
}
