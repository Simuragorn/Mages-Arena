using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class Shield : NetworkBehaviour
{
    public MagicType MagicTypeValue { get; private set; }
    [SerializeField] private MagicTypeEnum magicTypeEnum;
    public MagicTypeEnum MagicTypeEnum=> magicTypeEnum;
    public void Awake()
    {
        MagicTypeValue = MagicTypesManager.Singleton.GetMagicTypes().First(m => m.Type == magicTypeEnum);
        gameObject.layer = LayerMask.NameToLayer(MagicType.GetLayerName(magicTypeEnum, MagicEquipmentType.Shield));
    }

    private void FixedUpdate()
    {
        if (!IsOwner)
        {
            return;
        }
    }
}
