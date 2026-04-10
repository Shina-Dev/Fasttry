using UnityEngine;

public class Enemy : MonoBehaviour, IPooledObject
{
    [Header("Enemy Stats")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private int currentHealth;
    [SerializeField] private int scoreValue = 10;

    [Header("Shooting Settings")]
    [SerializeField] private float fireRate = 1.5f;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float targetYPosition = 4f;

    [Header("Enemy Type")]
    [SerializeField] private EnemyType enemyType = EnemyType.Shooter;

    [Header("Visual Settings")]
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite sniperSprite;

    private Rigidbody2D rb;
    private bool isActive = false;
    private bool hasReachedPosition = false;
    private bool IsFrozen = false;
    private float nextFireTime = 0f;
    private float moveSpeed = 3f;
    private Transform playerTransform;

    public enum EnemyType
    {
        Shooter,
        FastShooter,
        HeavyShooter,
        MovingShooter,
        Sniper
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (firePoint == null)
        {
            GameObject fp = new GameObject("FirePoint");
            fp.transform.SetParent(transform);
            fp.transform.localPosition = new Vector3(0, -0.6f, 0);
            firePoint = fp.transform;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    private void OnEnable()
    {
        isActive = true;
        currentHealth = maxHealth;
        hasReachedPosition = false;
        nextFireTime = Time.time + 1f;

        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        if (sprite != null)
        {
            sprite.color = Color.white;
        }

        StopAllCoroutines();
    }

    private void Update()
    {
        if (!isActive) return;
        if (GameManager.Instance == null || !GameManager.Instance.isGameActive) return;
        if (IsFrozen) return;

        if (hasReachedPosition)
        {
            if (Time.time >= nextFireTime)
            {
                Shoot();
                nextFireTime = Time.time + fireRate;
            }
        }
    }

    private void FixedUpdate()
    {
        if (!isActive) return;
        if (IsFrozen) { rb.linearVelocity = Vector2.zero; return; }

        if (!hasReachedPosition)
        {
            if (transform.position.y > targetYPosition)
            {
                rb.linearVelocity = Vector2.down * moveSpeed * 2f;
            }
            else
            {
                rb.linearVelocity = Vector2.zero;
                hasReachedPosition = true;
            }
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void Shoot()
    {
        if (firePoint == null)
        {
            Debug.LogWarning("FirePoint no asignado en Enemy!");
            return;
        }

        if (ObjectPool.Instance != null)
        {
            if (enemyType == EnemyType.Sniper)
            {
                ShootAtPlayer();
            }
            else
            {
                ShootStraight();
            }
        }
    }

    private void ShootStraight()
    {
        GameObject projectile = ObjectPool.Instance.SpawnFromPool(
            "EnemyProjectile",
            firePoint.position,
            Quaternion.identity
        );

        // ← AGREGAR: Sonido de disparo enemigo
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayEnemyShoot();
        }
    }

    private void ShootAtPlayer()
    {
        if (playerTransform == null)
        {
            ShootStraight();
            return;
        }

        Vector2 direction = (playerTransform.position - firePoint.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0, 0, angle - 90f);

        GameObject projectile = ObjectPool.Instance.SpawnFromPool(
            "EnemyProjectile",
            firePoint.position,
            rotation
        );

        if (projectile != null)
        {
            EnemyProjectile projScript = projectile.GetComponent<EnemyProjectile>();
            if (projScript != null)
            {
                projScript.SetDirection(direction);
                Debug.Log("🎯 Sniper disparó hacia el jugador!");
            }
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        // ← AGREGAR: Sonido de golpe
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayEnemyHit();
        }

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(FlashEffect());
            }
        }
    }

    private void Die()
    {
        // ← AGREGAR: Sonido de muerte
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayEnemyDeath();
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(scoreValue);
        }

        if (SlotMachine.Instance != null)
        {
            SlotMachine.Instance.AddChargeFromEvent(10f);
        }

        ReturnToPool();
    }

    private System.Collections.IEnumerator FlashEffect()
    {
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        if (sprite == null || !gameObject.activeInHierarchy)
        {
            yield break;
        }

        Color originalColor = sprite.color;
        sprite.color = Color.red;

        yield return new WaitForSeconds(0.1f);

        if (sprite != null && gameObject.activeInHierarchy)
        {
            sprite.color = originalColor;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.PlayerTakeDamage(2);
            }

            Die();
        }
    }

    public void OnObjectSpawn()
    {
        isActive = true;
        currentHealth = maxHealth;
        hasReachedPosition = false;

        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        if (sprite != null)
        {
            sprite.color = Color.white;
        }

        StopAllCoroutines();
    }

    private void ReturnToPool()
    {
        isActive = false;
        StopAllCoroutines();

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        if (ObjectPool.Instance != null)
        {
            ObjectPool.Instance.ReturnToPool(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public void SetEnemyType(EnemyType type)
    {
        enemyType = type;
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();

        switch (type)
        {
            case EnemyType.Shooter:
                fireRate = 1.5f;
                maxHealth = 2;
                scoreValue = 10;
                if (sprite != null && normalSprite != null)
                {
                    sprite.sprite = normalSprite;
                    sprite.color = Color.white;
                }
                break;

            case EnemyType.FastShooter:
                fireRate = 0.8f;
                maxHealth = 1;
                scoreValue = 15;
                if (sprite != null && normalSprite != null)
                {
                    sprite.sprite = normalSprite;
                    sprite.color = Color.white;
                }
                break;

            case EnemyType.HeavyShooter:
                fireRate = 2.5f;
                maxHealth = 5;
                scoreValue = 30;
                if (sprite != null && normalSprite != null)
                {
                    sprite.sprite = normalSprite;
                    sprite.color = new Color(0.7f, 0.7f, 0.7f);
                }
                break;

            case EnemyType.MovingShooter:
                fireRate = 1.2f;
                maxHealth = 3;
                scoreValue = 20;
                if (sprite != null && normalSprite != null)
                {
                    sprite.sprite = normalSprite;
                    sprite.color = Color.white;
                }
                break;

            case EnemyType.Sniper:
                fireRate = 2.0f;
                maxHealth = 3;
                scoreValue = 25;
                if (sprite != null && sniperSprite != null)
                {
                    sprite.sprite = sniperSprite;
                    sprite.color = new Color(1f, 0.9f, 0.3f);
                }
                Debug.Log("🎯 Sniper configurado con sprite especial");
                break;
        }

        currentHealth = maxHealth;
    }

    public void SetTargetPosition(float yPos)
    {
        targetYPosition = yPos;
    }

    private void OnDrawGizmos()
    {
        if (firePoint != null)
        {
            Gizmos.color = enemyType == EnemyType.Sniper ? Color.yellow : Color.red;
            Gizmos.DrawWireSphere(firePoint.position, 0.15f);
            Gizmos.DrawLine(firePoint.position, firePoint.position + Vector3.down * 0.5f);
        }
    }

    public void FreezeEnemy(float duration)
    {
        if (!IsFrozen && gameObject.activeInHierarchy)
            StartCoroutine(FreezeRoutine(duration));
    }

    private System.Collections.IEnumerator FreezeRoutine(float duration)
    {
        IsFrozen = true;
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();

        // Guardar color ANTES de congelar
        Color originalColor = sprite != null ? sprite.color : Color.white;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            if (sprite != null)
            {
                float t = Mathf.PingPong(elapsed * 8f, 1f);
                sprite.color = Color.Lerp(Color.cyan, Color.white, t);
            }
            yield return null;
        }

        IsFrozen = false;
        // Restaurar color original exacto
        if (sprite != null) sprite.color = originalColor;
    }
}