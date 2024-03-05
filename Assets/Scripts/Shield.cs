using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class Shield : NetworkBehaviour
{
    private Transform target;
    private bool isLaunched = false;
    public MagicType MagicTypeValue { get; private set; }
    public void Launch(Transform spawn, MagicTypeEnum magicTypeEnum)
    {
        target = spawn;
        isLaunched = true;
        MagicTypeValue = MagicTypesManager.Singleton.GetMagicTypes().First(m => m.Type == magicTypeEnum);
        gameObject.layer = LayerMask.NameToLayer(MagicType.GetLayerName(magicTypeEnum, MagicEquipmentType.Shield));
    }

    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }
        Move();
    }

    private void Move()
    {
        if (!isLaunched)
        {
            return;
        }
        transform.position = target.position;
        transform.rotation = target.rotation;
    }
}
