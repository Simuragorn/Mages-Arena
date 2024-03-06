using UnityEngine;


public enum MagicTypeEnum
{
    Fire,
    Electric
}

public enum MagicEquipmentType
{
    Shell,
    Shield
}

[CreateAssetMenu(menuName = "MagicType")]
public class MagicType : ScriptableObject
{
    [SerializeField] private MagicTypeEnum type;

    [SerializeField] private Shell shellPrefab;
    [SerializeField] private PhysicsMaterial2D shellPhysicsMaterial;
    [SerializeField] private float shootDelay;
    [SerializeField] private float shootImpulse;
    [SerializeField] private float shootManaCost;
    [SerializeField] private int ricochetCount;
    [SerializeField] private float shootSpreadAngle;

    [SerializeField] private Shield shieldPrefab;
    [SerializeField] private float shieldDelay;
    [SerializeField] private float shieldManaCost;

    public MagicTypeEnum Type => type;

    public Shell ShellPrefab => shellPrefab;
    public PhysicsMaterial2D ShellPhysicsMaterial => shellPhysicsMaterial;
    public float ShootDelay => shootDelay;
    public float ShootImpulse => shootImpulse;
    public float ShootManaCost => shootManaCost;
    public int RicochetCount => ricochetCount;
    public float ShootSpreadAngle => shootSpreadAngle;


    public Shield ShieldPrefab => shieldPrefab;
    public float ShieldDelay => shieldDelay;
    public float ShieldManaCost => shieldManaCost;

    public static string GetLayerName(MagicTypeEnum magicType, MagicEquipmentType equipmentType)
    {
        string layerName = $"{magicType}_{equipmentType}";
        return layerName;
    }
}
