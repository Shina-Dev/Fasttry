using UnityEngine;
using System.Collections.Generic;

public class SlotMachine : MonoBehaviour
{
    public static SlotMachine Instance { get; private set; }

    public enum SymbolType
    {
        Cherry,
        Star,
        Seven,
        Diamond
    }

    [Header("Slot Configuration")]
    [SerializeField] private int slotCount = 4;

    [Header("Spin Settings")]
    [SerializeField] private float spinChargeRequired = 100f;
    [SerializeField] private float spinChargeGainPerSecond = 2f;
    [SerializeField] private float spinCooldown = 2f;

    [Header("Current State")]
    public float currentSpinCharge = 0f;
    public SymbolType[] currentSlots;
    public bool isSpinning = false;

    private float lastSpinTime = 0f;
    private readonly int[] symbolWeights = { 40, 30, 20, 10 };
    private int totalWeight;

    public delegate void OnSpinComplete(SymbolType[] results);
    public event OnSpinComplete onSpinComplete;

    public delegate void OnChargeChanged(float charge, float maxCharge);
    public event OnChargeChanged onChargeChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        currentSlots = new SymbolType[slotCount];

        totalWeight = 0;
        foreach (int weight in symbolWeights)
        {
            totalWeight += weight;
        }
    }

    private void Start()
    {
        for (int i = 0; i < slotCount; i++)
        {
            currentSlots[i] = GetRandomSymbol();
        }

        Debug.Log("SlotMachine inicializado correctamente!");
    }


    private void Update()
    {
        // ✨ NO hacer nada si el juego no está activo
        if (GameManager.Instance == null || !GameManager.Instance.isGameActive)
        {
            return;
        }

        // Ganar carga pasiva
        GainCharge(spinChargeGainPerSecond * Time.deltaTime);

        // Input para hacer spin
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E))
        {
            TrySpin();
        }
    }

    public void GainCharge(float amount)
    {
        currentSpinCharge = Mathf.Min(currentSpinCharge + amount, spinChargeRequired);
        onChargeChanged?.Invoke(currentSpinCharge, spinChargeRequired);
    }

    public bool CanSpin()
    {
        return currentSpinCharge >= spinChargeRequired
            && !isSpinning
            && Time.time >= lastSpinTime + spinCooldown;
    }

    public void TrySpin()
    {
        if (!CanSpin())
        {
            Debug.Log($"No se puede hacer spin. Carga: {currentSpinCharge:F0}/{spinChargeRequired}");
            return;
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySpinButton();
        }

        PerformSpin();
    }

    private void PerformSpin()
    {
        Debug.Log("=== PERFORMING SPIN ===");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterSpinUsed();
        }


        isSpinning = true;
        currentSpinCharge = 0f;
        lastSpinTime = Time.time;

        for (int i = 0; i < slotCount; i++)
        {
            currentSlots[i] = GetRandomSymbol();
        }

        Debug.Log($"SPIN! Resultado: {GetCombinationString()}");

        onSpinComplete?.Invoke(currentSlots);

        EvaluateCombination();

        isSpinning = false;
        onChargeChanged?.Invoke(currentSpinCharge, spinChargeRequired);

        Debug.Log("Spin completado. Carga reseteada a 0.");
    }

    private SymbolType GetRandomSymbol()
    {
        int randomValue = Random.Range(0, totalWeight);
        int currentWeight = 0;

        for (int i = 0; i < symbolWeights.Length; i++)
        {
            currentWeight += symbolWeights[i];
            if (randomValue < currentWeight)
            {
                return (SymbolType)i;
            }
        }

        return SymbolType.Cherry;
    }

    private void EvaluateCombination()
    {
        Dictionary<SymbolType, int> symbolCount = new Dictionary<SymbolType, int>();
        foreach (SymbolType slot in currentSlots)
        {
            if (!symbolCount.ContainsKey(slot))
                symbolCount[slot] = 0;
            symbolCount[slot]++;
        }

        WeaponType weaponType = DetermineWeaponType(symbolCount);

        Debug.Log($"SlotMachine: Arma determinada = {weaponType}");

        if (WeaponManager.Instance != null)
        {
            WeaponManager.Instance.ActivateWeapon(weaponType, symbolCount);
        }
    }

    private WeaponType DetermineWeaponType(Dictionary<SymbolType, int> symbolCount)
    {
        int cherries = symbolCount.ContainsKey(SymbolType.Cherry) ? symbolCount[SymbolType.Cherry] : 0;
        int stars = symbolCount.ContainsKey(SymbolType.Star) ? symbolCount[SymbolType.Star] : 0;
        int sevens = symbolCount.ContainsKey(SymbolType.Seven) ? symbolCount[SymbolType.Seven] : 0;
        int diamonds = symbolCount.ContainsKey(SymbolType.Diamond) ? symbolCount[SymbolType.Diamond] : 0;

        // TIER 3 - ÉPICAS (Seven + Diamond)
        if (sevens == 4) return WeaponType.JackpotLaser;
        if (diamonds == 4) return WeaponType.DiamondShield;
        if (diamonds == 3 && sevens == 1) return WeaponType.EMPBomb;
        if (diamonds == 2 && sevens == 2) return WeaponType.CritCannon;
        if (diamonds == 1 && sevens == 3) return WeaponType.UltimateShot;

        // TIER 4 - COMODÍN (Diamond + otros)
        if (diamonds >= 2 && stars == 2) return WeaponType.ScatterStorm;
        if (diamonds >= 2 && cherries == 2) return WeaponType.RapidDiamond;
        if (diamonds == 1 && stars == 3) return WeaponType.SeekingStars;
        if (diamonds == 1 && cherries == 3) return WeaponType.CherryBomb;

        // TIER 2 - MEJORADAS (Star + Seven) → POWER-UPS DE VIDA
        if (stars == 4) return WeaponType.GuidedMissiles;
        if (cherries == 3 && sevens == 1) return WeaponType.HealthBoost;      // +1 vida
        if (stars == 3 && sevens == 1) return WeaponType.LaserBeam;
        if (stars == 2 && sevens == 2) return WeaponType.HealthRestore;       // +2 vidas
        if (stars == 1 && sevens == 3) return WeaponType.UltimateShot;

        // TIER 1 - BÁSICAS (Cherry + Star)
        if (cherries == 4) return WeaponType.QuadCherry;
        if (cherries == 3 && stars == 1) return WeaponType.CherryBoost;
        if (cherries == 2 && stars == 2) return WeaponType.TwinShot;
        if (cherries == 1 && stars == 3) return WeaponType.StarBurst;

        return WeaponType.Basic;
    }

    private string GetCombinationString()
    {
        string result = "";
        foreach (SymbolType slot in currentSlots)
        {
            result += GetSymbolEmoji(slot);
        }
        return result;
    }

    private string GetSymbolEmoji(SymbolType type)
    {
        switch (type)
        {
            case SymbolType.Cherry: return "🍒";
            case SymbolType.Star: return "⭐";
            case SymbolType.Seven: return "7️⃣";
            case SymbolType.Diamond: return "💎";
            default: return "?";
        }
    }

    public Color GetSymbolColor(SymbolType type)
    {
        switch (type)
        {
            case SymbolType.Cherry: return Color.red;
            case SymbolType.Star: return Color.yellow;
            case SymbolType.Seven: return Color.green;
            case SymbolType.Diamond: return Color.cyan;
            default: return Color.white;
        }
    }


    public void AddChargeFromEvent(float amount)
    {
        GainCharge(amount);
        Debug.Log($"+{amount}% Spin Charge! ({currentSpinCharge:F0}/{spinChargeRequired})");
    }

    public void ResetSlotMachine()
    {
        Debug.Log("🔄 Reseteando SlotMachine...");

        currentSpinCharge = 0f;
        isSpinning = false;
        lastSpinTime = 0f;

        // Resetear slots a aleatorios
        for (int i = 0; i < slotCount; i++)
        {
            currentSlots[i] = GetRandomSymbol();
        }

        // Notificar cambio de carga
        onChargeChanged?.Invoke(currentSpinCharge, spinChargeRequired);

        Debug.Log("✅ SlotMachine reseteada");
    }
}