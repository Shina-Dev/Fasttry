using UnityEngine;

public class EnemyProjectile : MonoBehaviour, IPooledObject
{
    [Header("Projectile Settings")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private int damage = 1;
    [SerializeField] private float lifetime = 5f;

    private float spawnTime;
    private Rigidbody2D rb;
    private Vector2 direction = Vector2.down;  // ← NUEVO: Dirección por defecto

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        spawnTime = Time.time;
        direction = Vector2.down;  // ← Resetear a dirección por defecto
    }

    private void Update()
    {
        // Auto-desactivarse después del lifetime
        if (Time.time - spawnTime > lifetime)
        {
            ReturnToPool();
        }

        // Auto-desactivarse si sale muy lejos de la pantalla
        if (transform.position.y < -10f || transform.position.y > 10f ||
            Mathf.Abs(transform.position.x) > 10f)
        {
            ReturnToPool();
        }
    }

    private void FixedUpdate()
    {
        // ← MODIFICADO: Mover en la dirección establecida
        rb.linearVelocity = direction * speed;
    }

    // ← NUEVO: Método para establecer dirección custom
    public void SetDirection(Vector2 newDirection)
    {
        direction = newDirection.normalized;
    }

    public void OnObjectSpawn()
    {
        spawnTime = Time.time;
        direction = Vector2.down;  // Resetear dirección
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Colisión con el jugador
        if (collision.CompareTag("Player"))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.PlayerTakeDamage(damage);
            }

            ReturnToPool();
        }

        // Colisión con proyectiles del jugador
        if (collision.CompareTag("Projectile"))
        {
            // Destruir el proyectil del jugador también
            collision.gameObject.SetActive(false);

            ReturnToPool();
        }
    }

    private void ReturnToPool()
    {
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

    public void SetDamage(int newDamage)
    {
        damage = newDamage;
    }

    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }
}