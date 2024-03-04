using TMPro;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI manaText;
    private PlayerMagic playerMagic;

    void Update()
    {
        if (playerMagic == null)
        {
            if (Player.LocalInstance == null)
            {
                return;
            }
            playerMagic = Player.LocalInstance.GetComponent<PlayerMagic>();
        }
        manaText.text = $"Mana: {playerMagic.ActualMana}";
    }
}
