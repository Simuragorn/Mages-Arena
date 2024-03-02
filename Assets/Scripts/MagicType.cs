using UnityEngine;

[CreateAssetMenu(menuName = "MagicType")]
public class MagicType : ScriptableObject
{
    [SerializeField] private string name;
    [SerializeField] private Shell shellPrefab;
    [SerializeField] private Shield shieldPrefab;
    [SerializeField] private float shootDelay;
    [SerializeField] private float shieldDelay;

    public string Name => name;
    public Shell ShellPrefab => shellPrefab;
    public Shield ShieldPrefab => shieldPrefab;
    public float ShootDelay => shootDelay;
    public float ShieldDelay => shieldDelay;
}
