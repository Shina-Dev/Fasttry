using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    [Header("Heart Sprites")]
    [SerializeField] private Sprite heartFull;
    [SerializeField] private Sprite heartEmpty;

    [Header("Heart Images")]
    [SerializeField] private Image[] hearts = new Image[4];

    [Header("Diamond Shield")]
    [SerializeField] private Image shieldIconEmpty; 
    [SerializeField] private Image shieldIconFull;

    private int lastKnownHealth = 3;
    private bool lastKnownShield = false;

    private void Start()
    {
        if (shieldIconEmpty != null)
            shieldIconEmpty.enabled = true;
        UpdateHearts();
        UpdateShield();
    }

    private void Update()
    {
        if (GameManager.Instance == null) return;

        if (GameManager.Instance.playerLives != lastKnownHealth)
        {
            lastKnownHealth = GameManager.Instance.playerLives;
            UpdateHearts();
        }

        // Verificar cambio de escudo
        bool shieldActive = WeaponManager.Instance != null && WeaponManager.Instance.IsInvincible();
        if (shieldActive != lastKnownShield)
        {
            lastKnownShield = shieldActive;
            UpdateShield();
        }
    }

    private void UpdateHearts()
    {
        if (GameManager.Instance == null) return;

        int currentLives = GameManager.Instance.playerLives;

        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] == null) continue;

            if (i < 3)
            {
                hearts[i].sprite = i < currentLives ? heartFull : heartEmpty;
                hearts[i].enabled = true;
                hearts[i].color = i < currentLives ? Color.white : new Color(1f, 1f, 1f, 0.3f);
            }
            else
            {
                if (currentLives >= 4)
                {

                    hearts[i].enabled = true;
                    hearts[i].color = Color.cyan;
                }
                else
                {
                    hearts[i].enabled = false;
                }
            }
        }
    }

    private void UpdateShield()
    {
        bool isActive = WeaponManager.Instance != null && WeaponManager.Instance.IsInvincible();

        // NUNCA tocar shieldIconEmpty aquí
        // Solo manejar el relleno
        if (shieldIconFull != null)
            shieldIconFull.enabled = isActive;
    }
}
