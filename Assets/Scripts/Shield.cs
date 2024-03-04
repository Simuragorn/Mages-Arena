using System.Linq;
using UnityEngine;

public class Shield : MonoBehaviour
{
    private Transform target;
    private bool isLaunched = false;
    public MagicType MagicType { get; private set; }
    public void Launch(Transform spawn, MagicTypeEnum magicTypeEnum)
    {
        target = spawn;
        isLaunched = true;
        MagicType = MagicTypesManager.Singleton.GetMagicTypes().First(m => m.Type == magicTypeEnum);
    }

    private void Update()
    {
        if (!isLaunched)
        {
            return;
        }
        transform.position = target.position;
        transform.rotation = target.rotation;
    }
}
