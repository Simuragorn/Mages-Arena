using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MagicTypesManager : NetworkBehaviour
{
    [SerializeField] private List<MagicType> magicTypes;
    public static MagicTypesManager Singleton;
    private void Awake()
    {
        var instances = FindObjectsByType<MagicTypesManager>(FindObjectsSortMode.None);
        if (instances.Length > 1)
        {
            Singleton = instances[0];
            if (IsServer)
            {
                Destroy(gameObject);
            }
            return;
        }
        Singleton = this;
    }
    public IReadOnlyList<MagicType> GetMagicTypes()
    {
        return magicTypes;
    }
}
