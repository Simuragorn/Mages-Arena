using UnityEngine;


public enum MagicTypeEnum
{
    Fire,
    Electric
}

[CreateAssetMenu(menuName = "MagicType")]
public class MagicType : ScriptableObject
{
    [SerializeField] private MagicTypeEnum type;

    [SerializeField] private Shell shellPrefab;
    [SerializeField] private float shootDelay;
    [SerializeField] private float shootSpeed;
    [SerializeField] private float shootManaCost;

    [SerializeField] private Shield shieldPrefab;    
    [SerializeField] private float shieldDelay;
    [SerializeField] private float shieldManaCost;

    public MagicTypeEnum Type => type;
    public Shell ShellPrefab => shellPrefab;
    public Shield ShieldPrefab => shieldPrefab;
    public float ShootDelay => shootDelay;
    public float ShieldDelay => shieldDelay;
    public float ShootSpeed => shootSpeed;
    public float ShootManaCost => shootManaCost;
    public float ShieldManaCost => shieldManaCost;
}
