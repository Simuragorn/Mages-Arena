using UnityEngine;

[CreateAssetMenu(menuName = "PlayerIdentity")]
public class PlayerIdentity : ScriptableObject
{
    [SerializeField] private string name;
    [SerializeField] private RuntimeAnimatorController animator;
    [SerializeField] private Sprite sprite;
    [SerializeField] private int index;

    public string Name => name;
    public int Index => index;
    public Sprite Sprite => sprite;
    public RuntimeAnimatorController Animator => animator;
}
