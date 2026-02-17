// Enum de todos los tipos de armas
// Este archivo debe estar en Assets/Scripts/Weapons/

public enum WeaponType
{
    // Básico
    Basic,

    // TIER 1 - Básicas
    QuadCherry,
    CherryBoost,
    TwinShot,
    StarBurst,

    // TIER 2 - Mejoradas
    GuidedMissiles,
    HealthBoost,      // ← NUEVO: +1 vida (antes PiercingShot)
    LaserBeam,
    HealthRestore,    // ← NUEVO: +2 vidas (antes NovaBlast)
    BonusLife,        // ← NUEVO: +1 vida extra 4ta (antes LuckyStrike)

    // TIER 3 - Épicas
    JackpotLaser,
    DiamondShield,
    EMPBomb,
    CritCannon,
    UltimateShot,

    // TIER 4 - Comodín
    ScatterStorm,
    RapidDiamond,
    SeekingStars,
    CherryBomb
}