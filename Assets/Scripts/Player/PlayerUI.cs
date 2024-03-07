using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private Image manaFillImage;
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
        manaFillImage.fillAmount = playerMagic.ActualMana / playerMagic.MaxMana;
    }
}
