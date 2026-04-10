using UnityEngine;

public class WeaponSystem : MonoBehaviour
{
    public static WeaponSystem Instance { get; private set; }

    [Header("Shooting Settings")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private float baseFireRate = 0.5f;

    [Header("Current Weapon")]
    [SerializeField] private WeaponType currentWeaponType = WeaponType.Basic;

    private float nextFireTime = 0f;
    private bool autoFire = true;
    private float fireRateMultiplier = 1f;
    private float damageMultiplier = 1f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void Update()
    {
        if (GameManager.Instance == null) return;
        if (!GameManager.Instance.isGameActive) return;

        float currentFireRate = baseFireRate / fireRateMultiplier;

        if (autoFire)
        {
            if (Time.time >= nextFireTime)
            {
                Shoot();
                nextFireTime = Time.time + currentFireRate;
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            {
                if (Time.time >= nextFireTime)
                {
                    Shoot();
                    nextFireTime = Time.time + currentFireRate;
                }
            }
        }
    }

    private void Shoot()
    {
        if (firePoint == null)
        {
            Debug.LogWarning("FirePoint no asignado!");
            return;
        }

        switch (currentWeaponType)
        {
            case WeaponType.HealthBoost:
            case WeaponType.HealthRestore:
            case WeaponType.Basic:
            case WeaponType.QuadCherry:
            case WeaponType.CherryBoost:
            case WeaponType.GuidedMissiles:
            case WeaponType.CritCannon:
            case WeaponType.CherryBomb:
            case WeaponType.UltimateShot:
            case WeaponType.EMPBomb:
                ShootBasic();
                break;

            case WeaponType.TwinShot:
            case WeaponType.RapidDiamond:
                ShootDouble();
                break;

            case WeaponType.StarBurst:
                ShootTriple();
                break;

            case WeaponType.SeekingStars:
                StartCoroutine(ShootSeekingStars());
                break;

            case WeaponType.ScatterStorm:
                ShootSpread();
                break;

            case WeaponType.LaserBeam:
            case WeaponType.JackpotLaser:
                ShootLaser();
                break;

            case WeaponType.DiamondShield:
                ShootBasic();
                break;

            default:
                Debug.LogWarning($"Tipo de arma no manejado: {currentWeaponType}");
                ShootBasic();
                break;
        }
    }

    private void ShootBasic()
    {
        SpawnProjectile(firePoint.position, firePoint.rotation);
    }

    private void ShootDouble()
    {
        Vector3 offset = firePoint.right * 0.3f;
        SpawnProjectile(firePoint.position + offset, firePoint.rotation);
        SpawnProjectile(firePoint.position - offset, firePoint.rotation);
    }

    private void ShootTriple()
    {
        SpawnProjectile(firePoint.position, firePoint.rotation);
        Vector3 offset = firePoint.right * 0.4f;
        SpawnProjectile(firePoint.position + offset, firePoint.rotation);
        SpawnProjectile(firePoint.position - offset, firePoint.rotation);
    }

    private void ShootSpread()
    {
        int count = 7;
        float spreadAngle = 40f;
        float angleStep = spreadAngle / (count - 1);
        float startAngle = -spreadAngle / 2f;

        for (int i = 0; i < count; i++)
        {
            float angle = startAngle + angleStep * i;
            Quaternion rotation = firePoint.rotation * Quaternion.Euler(0, 0, angle);
            SpawnProjectile(firePoint.position, rotation);
        }
    }

    private System.Collections.IEnumerator ShootSeekingStars()
    {
        for (int i = 0; i < 3; i++)
        {
            SpawnProjectile(firePoint.position, firePoint.rotation);
            yield return new WaitForSeconds(0.12f);
        }
    }

    private void ShootLaser()
    {
        for (int i = 0; i < 3; i++)
        {
            SpawnProjectile(firePoint.position + firePoint.up * i * 0.2f, firePoint.rotation);
        }
    }

    private void SpawnProjectile(Vector3 position, Quaternion rotation)
    {
        if (ObjectPool.Instance == null) return;

        GameObject projectileGO = ObjectPool.Instance.SpawnFromPool("Projectile", position, rotation);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayPlayerShoot();

        if (projectileGO == null) return;

        Projectile proj = projectileGO.GetComponent<Projectile>();
        if (proj == null) return;

        // Daño base
        proj.SetDamage(Mathf.RoundToInt(1 * damageMultiplier));

        // Resetear propiedades especiales
        proj.SetHoming(false);
        proj.SetPiercing(false);
        proj.SetSplash(false);

        // Aplicar propiedades según arma
        switch (currentWeaponType)
        {
            case WeaponType.GuidedMissiles:
                proj.SetHoming(true, 5f);
                break;

            case WeaponType.SeekingStars:
                proj.SetHomingSoft(true);
                break;

            case WeaponType.CritCannon:
                proj.SetColorOverride(new Color(1f, 0.5f, 0f));
                break;

            case WeaponType.UltimateShot:
                proj.SetColorOverride(new Color(1f, 0.15f, 0.15f));
                break;

            case WeaponType.RapidDiamond:
                proj.SetPiercing(true);
                break;

            case WeaponType.CherryBomb:
                proj.SetSplash(true, 1.5f, Mathf.RoundToInt(1 * damageMultiplier));
                break;
        }
    }

    public void SetWeaponType(WeaponType newType)
    {
        Debug.Log($"WeaponSystem: Cambiando de {currentWeaponType} a {newType}");
        currentWeaponType = newType;
    }

    public void SetFireRateMultiplier(float multiplier)
    {
        fireRateMultiplier = multiplier;
    }

    public void SetDamageMultiplier(float multiplier)
    {
        damageMultiplier = multiplier;
    }

    public void SetAutoFire(bool active)
    {
        autoFire = active;
    }

    private void OnDrawGizmos()
    {
        if (firePoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(firePoint.position, 0.1f);
            Gizmos.DrawLine(firePoint.position, firePoint.position + firePoint.up * 0.5f);
        }
    }
}