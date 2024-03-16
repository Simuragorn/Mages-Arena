using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private Image manaFillImage;
    private PlayerMagic playerMagic;

    [SerializeField] private Image staminaFillImage;
    private PlayerMovement playerMovement;

    void Update()
    {
        HandleMagic();
        HandleStamina();
    }

    private void HandleMagic()
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

    private void HandleStamina()
    {
        if (playerMovement == null)
        {
            if (Player.LocalInstance == null)
            {
                return;
            }
            playerMovement = Player.LocalInstance.GetComponent<PlayerMovement>();
        }
        staminaFillImage.fillAmount = playerMovement.ActualStamina / playerMovement.MaxStamina;
    }
}
