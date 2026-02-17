using UnityEngine;
using System.Collections.Generic;

public class WeaponManager : MonoBehaviour
{
    public static WeaponManager Instance { get; private set; }

    [Header("Current Weapon")]
    public WeaponType currentWeaponType = WeaponType.Basic;
    public float currentWeaponDuration = 20f;
    public bool hasActiveWeapon = false;

    private const float WEAPON_DURATION = 20f;

    [Header("Weapon Stats Modifiers")]
    private float damageMultiplier = 1f;
    private float fireRateMultiplier = 1f;
    private int projectileCount = 1;
    private bool isInvincible = false;

    private float weaponActivationTime;
    private Dictionary<SlotMachine.SymbolType, int> lastCombination;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        // Verificar si el arma actual ha expirado
        if (hasActiveWeapon && currentWeaponType != WeaponType.Basic)
        {
            float timeActive = Time.time - weaponActivationTime;

            if (timeActive > WEAPON_DURATION)
            {
                Debug.Log($"Arma {currentWeaponType} expiró después de {WEAPON_DURATION}s");
                DeactivateWeapon();
            }
        }
    }

    public void ActivateWeapon(WeaponType weaponType, Dictionary<SlotMachine.SymbolType, int> combination)
    {
        Debug.Log($"====> ACTIVANDO ARMA: {weaponType} <====");

        // Si había un arma activa, desactivarla primero
        if (hasActiveWeapon && currentWeaponType != WeaponType.Basic)
        {
            Debug.Log($"Desactivando arma anterior: {currentWeaponType}");
        }

        currentWeaponType = weaponType;
        lastCombination = combination;
        hasActiveWeapon = true;
        weaponActivationTime = Time.time;

        // Resetear modificadores
        ResetModifiers();

        // Configurar según el tipo de arma
        ConfigureWeapon(weaponType);

        // IMPORTANTE: Aplicar power-ups de vida INMEDIATAMENTE
        ApplyHealthPowerUps(weaponType);

        // Aplicar a WeaponSystem
        if (WeaponSystem.Instance != null)
        {
            WeaponSystem.Instance.SetWeaponType(weaponType);
            WeaponSystem.Instance.SetFireRateMultiplier(fireRateMultiplier);
            WeaponSystem.Instance.SetDamageMultiplier(damageMultiplier);

            Debug.Log($"Arma aplicada a WeaponSystem: {weaponType}");
        }
        else
        {
            Debug.LogError("WeaponSystem.Instance es NULL!");
        }

        Debug.Log($"Arma activada: {weaponType} - Duración: {currentWeaponDuration}s - FireRate: x{fireRateMultiplier}");
    }

    private void ApplyHealthPowerUps(WeaponType weaponType)
    {
        switch (weaponType)
        {
            case WeaponType.HealthBoost:
                // 🍒🍒🍒7️⃣ → +1 vida (hasta 3 máx)
                GiveHealth(1, allowBonus: false);
                Debug.Log("💚 HEALTH BOOST: +1 vida!");
                break;

            case WeaponType.HealthRestore:
                // ⭐⭐7️⃣7️⃣ → +2 vidas (hasta 3 máx)
                GiveHealth(2, allowBonus: false);
                Debug.Log("💚💚 HEALTH RESTORE: +2 vidas!");
                break;

            case WeaponType.BonusLife:
                // ⭐7️⃣7️⃣7️⃣ → +1 vida extra (4ta vida)
                GiveHealth(1, allowBonus: true);
                Debug.Log("💙 BONUS LIFE: ¡Vida extra desbloqueada!");
                break;
        }
    }

    private void ConfigureWeapon(WeaponType weaponType)
    {
        // Duración base: 20 segundos para todas las armas
        currentWeaponDuration = WEAPON_DURATION;

        switch (weaponType)
        {
            // TIER 1 - BÁSICAS (Cherry + Star)
            case WeaponType.QuadCherry:
                projectileCount = 4;
                fireRateMultiplier = 1.2f;
                break;

            case WeaponType.CherryBoost:
                projectileCount = 1;
                fireRateMultiplier = 1.5f;
                break;

            case WeaponType.TwinShot:
                projectileCount = 2;
                break;

            case WeaponType.StarBurst:
                projectileCount = 3;
                fireRateMultiplier = 0.9f;
                break;

            // TIER 2 - MEJORADAS (Star + Seven)
            case WeaponType.GuidedMissiles:
                projectileCount = 1;
                damageMultiplier = 1.5f;
                break;

            case WeaponType.HealthBoost:
                // Power-up de vida: no dispara, solo da +1 vida
                currentWeaponDuration = 0f;  // Efecto instantáneo
                break;

            case WeaponType.LaserBeam:
                currentWeaponDuration = 3f;
                damageMultiplier = 2f;
                fireRateMultiplier = 3f;
                break;

            case WeaponType.HealthRestore:
                // Power-up de vida: no dispara, solo da +2 vidas
                currentWeaponDuration = 0f;  // Efecto instantáneo
                break;

            case WeaponType.BonusLife:
                // Power-up de vida: no dispara, da +1 vida extra (4ta)
                currentWeaponDuration = 0f;  // Efecto instantáneo
                break;

            // TIER 3 - ÉPICAS (Seven + Diamond)
            case WeaponType.JackpotLaser:
                currentWeaponDuration = 5f;
                damageMultiplier = 5f;
                fireRateMultiplier = 5f;
                break;

            case WeaponType.DiamondShield:
                currentWeaponDuration = 5f;
                isInvincible = true;
                fireRateMultiplier = 2f;
                break;

            case WeaponType.EMPBomb:
                currentWeaponDuration = 4f;
                break;

            case WeaponType.CritCannon:
                damageMultiplier = 3f;
                break;

            case WeaponType.UltimateShot:
                damageMultiplier = 4f;
                projectileCount = 1;
                break;

            // TIER 4 - COMODÍN (Diamond + otros)
            case WeaponType.ScatterStorm:
                projectileCount = 7;
                fireRateMultiplier = 0.8f;
                break;

            case WeaponType.RapidDiamond:
                projectileCount = 2;
                fireRateMultiplier = 3f;
                break;

            case WeaponType.SeekingStars:
                projectileCount = 3;
                damageMultiplier = 1.5f;
                break;

            case WeaponType.CherryBomb:
                damageMultiplier = 2f;
                break;

            default:
                projectileCount = 1;
                break;
        }
    }

    private void ResetModifiers()
    {
        damageMultiplier = 1f;
        fireRateMultiplier = 1f;
        projectileCount = 1;
        isInvincible = false;
    }

    private void DeactivateWeapon()
    {
        Debug.Log($"Arma {currentWeaponType} desactivada");

        currentWeaponType = WeaponType.Basic;
        hasActiveWeapon = false;
        ResetModifiers();

        if (WeaponSystem.Instance != null)
        {
            WeaponSystem.Instance.SetWeaponType(WeaponType.Basic);
            WeaponSystem.Instance.SetFireRateMultiplier(1f);
            WeaponSystem.Instance.SetDamageMultiplier(1f);
        }
    }

    // Getters públicos
    public float GetDamageMultiplier() => damageMultiplier;
    public float GetFireRateMultiplier() => fireRateMultiplier;
    public int GetProjectileCount() => projectileCount;
    public bool IsInvincible() => isInvincible;

    public float GetWeaponTimeRemaining()
    {
        if (!hasActiveWeapon || currentWeaponType == WeaponType.Basic)
            return 0f;

        float elapsed = Time.time - weaponActivationTime;
        return Mathf.Max(0f, WEAPON_DURATION - elapsed);
    }

    private void GiveHealth(int amount, bool allowBonus = false)
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager.Instance es NULL! No se puede dar vida.");
            return;
        }

        int maxAllowed = allowBonus ? 4 : 3;  // 3 vidas normales, o 4 con bonus
        int currentLives = GameManager.Instance.playerLives;

        if (currentLives < maxAllowed)
        {
            int livesToAdd = Mathf.Min(amount, maxAllowed - currentLives);
            GameManager.Instance.playerLives += livesToAdd;

            Debug.Log($"💚 ¡VIDA RESTAURADA! +{livesToAdd} vida(s). Total: {GameManager.Instance.playerLives}/{maxAllowed}");
        }
        else
        {
            Debug.Log($"❌ Vida ya está al máximo! ({currentLives}/{maxAllowed})");
        }
    }
    public void ResetWeaponManager()
    {
        Debug.Log("🔄 Reseteando WeaponManager...");

        currentWeaponType = WeaponType.Basic;
        hasActiveWeapon = false;
        currentWeaponDuration = 20f;
        weaponActivationTime = 0f;

        ResetModifiers();

        if (WeaponSystem.Instance != null)
        {
            WeaponSystem.Instance.SetWeaponType(WeaponType.Basic);
            WeaponSystem.Instance.SetFireRateMultiplier(1f);
            WeaponSystem.Instance.SetDamageMultiplier(1f);
        }

        Debug.Log("✅ WeaponManager reseteado");
    }

}

