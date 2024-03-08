using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MagicTypesManager : NetworkSingleton<MagicTypesManager>
{
    [SerializeField] private List<MagicType> magicTypes;
    public IReadOnlyList<MagicType> GetMagicTypes()
    {
        return magicTypes;
    }
}
