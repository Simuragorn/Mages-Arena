using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;

public class MagicTypesManager : NetworkSingleton<MagicTypesManager>
{
    [SerializeField] private List<MagicType> magicTypes;
    public IReadOnlyList<MagicType> GetMagicTypes()
    {
        return magicTypes;
    }
}
