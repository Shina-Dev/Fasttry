using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class SlotUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image[] slotImages = new Image[4];
    [SerializeField] private TextMeshProUGUI chargeText;
    [SerializeField] private Image chargeFillBar;
    [SerializeField] private TextMeshProUGUI weaponText;
    [SerializeField] private TextMeshProUGUI distanceText;
    [SerializeField] private TextMeshProUGUI difficultyText;
    [SerializeField] private TextMeshProUGUI weaponDurationText;
    [SerializeField] private Image weaponDurationBar;

    [Header("Slot Sprites")]
    [SerializeField] private Sprite cherrySprite;
    [SerializeField] private Sprite starSprite;
    [SerializeField] private Sprite sevenSprite;
    [SerializeField] private Sprite diamondSprite;
    [SerializeField] private Sprite questionSprite;

    [Header("Animation")]
    [SerializeField] private float spinAnimDuration = 0.5f;
    [SerializeField] private AnimationCurve spinCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private bool slotsAreQuestion = false;

    // ← NUEVO: Guardar escalas originales una sola vez
    private Vector3[] originalScales = new Vector3[4];
    private bool scalesInitialized = false;

    private void OnEnable()
    {
        StartCoroutine(SubscribeToSlotMachine());
    }

    private IEnumerator SubscribeToSlotMachine()
    {
        while (SlotMachine.Instance == null)
        {
            yield return new WaitForSeconds(0.1f);
        }

        Debug.Log("SlotUI: SlotMachine encontrado, suscribiendo eventos...");

        SlotMachine.Instance.onChargeChanged += UpdateChargeDisplay;
        SlotMachine.Instance.onSpinComplete += OnSpinComplete;

        // ← NUEVO: Inicializar escalas originales
        InitializeOriginalScales();

        HideAllSlots();

        Debug.Log("SlotUI: Todo listo!");
    }

    // ← NUEVO: Método para guardar escalas originales
    private void InitializeOriginalScales()
    {
        if (!scalesInitialized)
        {
            for (int i = 0; i < slotImages.Length; i++)
            {
                if (slotImages[i] != null)
                {
                    originalScales[i] = slotImages[i].transform.localScale;
                }
            }
            scalesInitialized = true;
            Debug.Log("✅ Escalas originales guardadas");
        }
    }

    // ← NUEVO: Método para resetear todas las escalas
    private void ResetAllScales()
    {
        for (int i = 0; i < slotImages.Length; i++)
        {
            if (slotImages[i] != null && i < originalScales.Length)
            {
                slotImages[i].transform.localScale = originalScales[i];
            }
        }
    }

    private void HideAllSlots()
    {
        // ← AGREGAR: Resetear escalas antes de ocultar
        ResetAllScales();

        // Mostrar símbolo de pregunta
        foreach (Image slotImage in slotImages)
        {
            if (slotImage != null)
            {
                slotImage.enabled = true;
                slotImage.sprite = questionSprite;
                slotImage.color = Color.white;
            }
        }
    }

    private void Update()
    {
        // Actualizar arma actual
        if (WeaponManager.Instance != null && weaponText != null)
        {
            string weaponName = WeaponManager.Instance.currentWeaponType.ToString();
            weaponText.text = $"{weaponName}";
        }

        // Actualizar distancia en KILÓMETROS
        if (GameManager.Instance != null && distanceText != null)
        {
            float meters = GameManager.Instance.distanceTraveled;  // ← SIN multiplicar por 10
            float km = meters / 1000f;

            if (km < 0.01f)
                distanceText.text = $"{meters:F0} M";
            else if (km < 0.1f)
                distanceText.text = $"{km:F2} KM";
            else if (km < 1f)
                distanceText.text = $"{km:F1} KM";
            else
                distanceText.text = $"{km:F1} KM";
        }

        // Actualizar dificultad
        if (GameManager.Instance != null && difficultyText != null)
        {
            float distance = GameManager.Instance.distanceTraveled;
            string difficulty;
            Color diffColor;

            if (distance < 300f)  // ← 0-300 unidades
            {
                difficulty = "EASY";
                diffColor = Color.green;
            }
            else if (distance < 700f)  // ← 300-700 unidades
            {
                difficulty = "MEDIUM";
                diffColor = Color.yellow;
            }
            else if (distance < 1300f)  // ← 700-1300 unidades
            {
                difficulty = "HARD";
                diffColor = new Color(1f, 0.5f, 0f);
            }
            else  // ← 1300+ unidades
            {
                difficulty = "EXTREME";
                diffColor = Color.red;
            }

            difficultyText.text = difficulty;
            difficultyText.color = diffColor;
        }

        // Actualizar barra de duración de arma
        if (WeaponManager.Instance != null)
        {
            float timeRemaining = WeaponManager.Instance.GetWeaponTimeRemaining();

            if (weaponDurationText != null)
            {
                if (timeRemaining > 0)
                {
                    weaponDurationText.text = $"{timeRemaining:F1}s";
                }
                else
                {
                    weaponDurationText.text = "";
                }
            }

            if (weaponDurationBar != null)
            {
                weaponDurationBar.fillAmount = timeRemaining / 20f;

                if (timeRemaining < 5f)
                    weaponDurationBar.color = Color.red;
                else if (timeRemaining < 10f)
                    weaponDurationBar.color = Color.yellow;
                else
                    weaponDurationBar.color = Color.green;
            }

            // Volver slots a "?" cuando expira el arma
            if (WeaponManager.Instance.currentWeaponType == WeaponType.Basic && !slotsAreQuestion)
            {
                HideAllSlots();
                slotsAreQuestion = true;
            }
        }
    }

    private void UpdateChargeDisplay(float current, float max)
    {
        if (chargeText != null)
        {
            float percentage = (current / max) * 100f;
            chargeText.text = $"SPIN: {percentage:F0}%";

            if (percentage >= 100f)
            {
                chargeText.color = Color.darkRed;
            }
            else if (percentage >= 50f)
            {
                chargeText.color = Color.yellow;
            }
            else
            {
                chargeText.color = Color.white;
            }
        }

        if (chargeFillBar != null)
        {
            chargeFillBar.fillAmount = current / max;
            chargeFillBar.color = current >= max ? Color.green : Color.cyan;
        }
    }

    private void OnSpinComplete(SlotMachine.SymbolType[] results)
    {
        Debug.Log("====> SlotUI: SPIN COMPLETE! <====");
        slotsAreQuestion = false;

        // ← NUEVO: Detener animaciones previas y resetear escalas
        StopAllCoroutines();
        ResetAllScales();

        StartCoroutine(AnimateSpin(results));
    }

    private IEnumerator AnimateSpin(SlotMachine.SymbolType[] results)
    {
        foreach (Image slotImage in slotImages)
        {
            if (slotImage != null)
            {
                slotImage.enabled = true;
            }
        }

        for (int i = 0; i < slotImages.Length && i < results.Length; i++)
        {
            if (slotImages[i] != null)
            {
                StartCoroutine(SpinSlot(i, results[i], i * 0.1f));
            }
        }

        yield return new WaitForSeconds(spinAnimDuration + 0.4f);
        UpdateSlotDisplay(results);
    }

    // ← MODIFICADO: Usar el índice en vez de pasar la imagen directamente
    private IEnumerator SpinSlot(int slotIndex, SlotMachine.SymbolType finalSymbol, float delay)
    {
        yield return new WaitForSeconds(delay);

        Image slotImage = slotImages[slotIndex];
        if (slotImage == null) yield break;

        float elapsed = 0f;
        // ← ARREGLADO: Usar la escala original guardada
        Vector3 originalScale = originalScales[slotIndex];

        while (elapsed < spinAnimDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / spinAnimDuration;

            float scale = Mathf.Abs(Mathf.Sin(progress * Mathf.PI * 3f));
            slotImage.transform.localScale = originalScale * (0.5f + scale * 0.5f);

            if (progress < 0.8f)
            {
                int randomSymbol = Random.Range(0, 4);
                slotImage.sprite = GetSpriteForSymbol((SlotMachine.SymbolType)randomSymbol);
            }

            yield return null;
        }

        // ← ARREGLADO: Restaurar escala original correcta
        slotImage.transform.localScale = originalScale;
        slotImage.sprite = GetSpriteForSymbol(finalSymbol);

        StartCoroutine(PopEffect(slotIndex));
    }

    // ← MODIFICADO: Usar índice en vez de Transform
    private IEnumerator PopEffect(int slotIndex)
    {
        Transform target = slotImages[slotIndex].transform;
        // ← ARREGLADO: Usar la escala original guardada
        Vector3 originalScale = originalScales[slotIndex];

        target.localScale = originalScale * 1.3f;

        float duration = 0.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            target.localScale = Vector3.Lerp(originalScale * 1.3f, originalScale, t);
            yield return null;
        }

        // ← ARREGLADO: Restaurar escala original correcta
        target.localScale = originalScale;
    }

    private void UpdateSlotDisplay(SlotMachine.SymbolType[] slots)
    {
        for (int i = 0; i < slotImages.Length && i < slots.Length; i++)
        {
            if (slotImages[i] != null)
            {
                slotImages[i].sprite = GetSpriteForSymbol(slots[i]);

                if (slots[i] == SlotMachine.SymbolType.Diamond)
                {
                    slotImages[i].color = Color.cyan;
                }
                else if (slots[i] == SlotMachine.SymbolType.Seven)
                {
                    slotImages[i].color = new Color(1f, 0.8f, 0f);
                }
                else
                {
                    slotImages[i].color = Color.white;
                }
            }
        }
    }

    private Sprite GetSpriteForSymbol(SlotMachine.SymbolType type)
    {
        switch (type)
        {
            case SlotMachine.SymbolType.Cherry: return cherrySprite;
            case SlotMachine.SymbolType.Star: return starSprite;
            case SlotMachine.SymbolType.Seven: return sevenSprite;
            case SlotMachine.SymbolType.Diamond: return diamondSprite;
            default: return null;
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        // ← AGREGAR: Resetear escalas al desactivar
        ResetAllScales();
    }

    private void OnDestroy()
    {
        if (SlotMachine.Instance != null)
        {
            SlotMachine.Instance.onChargeChanged -= UpdateChargeDisplay;
            SlotMachine.Instance.onSpinComplete -= OnSpinComplete;
        }
    }
}