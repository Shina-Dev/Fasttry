using UnityEngine;

public class Projectile : MonoBehaviour, IPooledObject
{
    [Header("Projectile Settings")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private int damage = 1;
    [SerializeField] private float lifetime = 3f;  // Tiempo antes de desactivarse

    private float spawnTime;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        spawnTime = Time.time;
    }

    private void Update()
    {
        // Auto-desactivarse después del lifetime
        if (Time.time - spawnTime > lifetime)
        {
            ReturnToPool();
        }
    }

    private void FixedUpdate()
    {
        // Mover hacia adelante (arriba en 2D)
        rb.linearVelocity = transform.up * speed;
    }

    public void OnObjectSpawn()
    {
        // Se llama cuando el objeto sale del pool
        spawnTime = Time.time;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Colisión con enemigos
        if (collision.CompareTag("Enemy"))
        {
            Debug.Log("Proyectil tocó enemigo!");  // ← AGREGAR ESTO

            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null)
            {
                Debug.Log("Enemy script encontrado, llamando TakeDamage");  // ← AGREGAR ESTO
                enemy.TakeDamage(damage);
            }
            else
            {
                Debug.LogWarning("Enemy NO tiene script Enemy!");  // ← AGREGAR ESTO
            }

            ReturnToPool();
        }

        // Colisión con obstáculos
        if (collision.CompareTag("Obstacle"))
        {
            Debug.Log("Proyectil impactó obstáculo");
            ReturnToPool();
        }
    }

    private void ReturnToPool()
    {
        // Resetear velocidad
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        // Devolver al pool
        if (ObjectPool.Instance != null)
        {
            ObjectPool.Instance.ReturnToPool(gameObject);
        }
        else
        {
            // Fallback si no hay pool
            gameObject.SetActive(false);
        }
    }

    // Método público para configurar el daño desde afuera
    public void SetDamage(int newDamage)
    {
        damage = newDamage;
    }

    // Método público para configurar la velocidad
    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }
}