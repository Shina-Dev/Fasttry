using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    [Header("Heart Sprites")]
    [SerializeField] private Sprite heartFull;
    [SerializeField] private Sprite heartEmpty;
    [SerializeField] private Sprite heartBonus;  // Corazón extra (diferente color)

    [Header("Heart Images")]
    [SerializeField] private Image[] hearts = new Image[4];  // 3 normales + 1 bonus

    private int lastKnownHealth = 3;

    private void Start()
    {
        UpdateHearts();
    }

    private void Update()
    {
        if (GameManager.Instance == null) return;

        // Solo actualizar si la vida cambió
        if (GameManager.Instance.playerLives != lastKnownHealth)
        {
            lastKnownHealth = GameManager.Instance.playerLives;
            UpdateHearts();
        }
    }

    private void UpdateHearts()
    {
        if (GameManager.Instance == null) return;

        int currentLives = GameManager.Instance.playerLives;

        // Actualizar cada corazón
        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] == null) continue;

            if (i < 3)
            {
                // Corazones normales (1-3)
                if (i < currentLives)
                {
                    hearts[i].sprite = heartFull;
                    hearts[i].enabled = true;
                    hearts[i].color = Color.white;
                }
                else
                {
                    hearts[i].sprite = heartEmpty;
                    hearts[i].enabled = true;
                    hearts[i].color = new Color(1f, 1f, 1f, 0.3f);  // Semi-transparente
                }
            }
            else
            {
                // Corazón bonus (4to)
                if (currentLives >= 4)
                {
                    hearts[i].sprite = heartBonus;
                    hearts[i].enabled = true;
                    hearts[i].color = Color.cyan;  // Color especial
                }
                else
                {
                    hearts[i].enabled = false;  // Oculto si no se tiene
                }
            }
        }
    }
}