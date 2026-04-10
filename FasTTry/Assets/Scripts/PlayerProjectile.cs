using UnityEngine;

public class Projectile : MonoBehaviour, IPooledObject
{
    [Header("Projectile Settings")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private int damage = 1;
    [SerializeField] private float lifetime = 1.5f;

    [Header("Homing")]
    private bool isHoming = false;
    private float homingStrength = 5f; // qué tan rápido gira hacia el enemigo
    private Transform homingTarget;

    [Header("Color Override")]
    private bool hasColorOverride = false;
    private Color projectileColor = Color.white;

    [Header("Piercing")]
    private bool isPiercing = false;

    [Header("Splash")]
    private bool isSplash = false;
    private float splashRadius = 1.5f;
    private int splashDamage = 1;

    private float spawnTime;
    private Rigidbody2D rb;
    private SpriteRenderer sr;

    [Header("Explosion Effect")]
    [SerializeField] private GameObject explosionEffectPrefab;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        spawnTime = Time.time;
        // Resetear todo al salir del pool
        isHoming = false;
        isPiercing = false;
        isSplash = false;
        hasColorOverride = false;
        homingTarget = null;
        if (sr != null) sr.color = Color.white;
    }

    private void Update()
    {
        if (Time.time - spawnTime > lifetime)
            ReturnToPool();
    }

    private void FixedUpdate()
    {
        if (isHoming)
        {
            if (homingTarget == null || !homingTarget.gameObject.activeInHierarchy)
                homingTarget = FindNearestEnemy();

            if (homingTarget != null)
            {
                Vector2 dirToTarget = ((Vector2)homingTarget.position - rb.position).normalized;
                float rotateAmount = Vector3.Cross(transform.up, dirToTarget).z;

                // Limitar la velocidad de rotación máxima
                float targetAngularVelocity = rotateAmount * homingStrength * Mathf.Rad2Deg;
                rb.angularVelocity = Mathf.Clamp(targetAngularVelocity, -200f, 200f);
            }
            else
            {
                rb.angularVelocity = 0f;
            }
        }

        rb.linearVelocity = transform.up * speed;
    }

    private Transform FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Transform nearest = null;
        float minDist = Mathf.Infinity;

        foreach (GameObject enemy in enemies)
        {
            if (!enemy.activeInHierarchy) continue;
            float dist = Vector2.Distance(transform.position, enemy.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = enemy.transform;
            }
        }

        return nearest;
    }

    public void OnObjectSpawn()
    {
        spawnTime = Time.time;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null)
                enemy.TakeDamage(damage);

            if (isSplash)
                ApplySplashDamage();

            if (!isPiercing)
                ReturnToPool();
        }

        if (collision.CompareTag("Obstacle"))
            ReturnToPool();
    }

    private void ApplySplashDamage()
    {
        // Daño en área
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, splashRadius);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                Enemy enemy = hit.GetComponent<Enemy>();
                if (enemy != null)
                    enemy.TakeDamage(splashDamage);
            }
        }

        // Efecto visual creado 100% por código
        SpawnExplosionEffect(transform.position);
    }

    private void SpawnExplosionEffect(Vector3 position)
    {
        GameObject effect = new GameObject("ExplosionWave");
        effect.transform.position = position;

        // Crear círculo con mesh puro
        MeshFilter mf = effect.AddComponent<MeshFilter>();
        MeshRenderer mr = effect.AddComponent<MeshRenderer>();

        // Material simple sin textura
        mr.material = new Material(Shader.Find("Sprites/Default"));
        mr.sortingOrder = 3;

        // Generar mesh de círculo
        mf.mesh = CreateCircleMesh(1f, 32);

        effect.AddComponent<ExplosionEffect>();
    }

    private Mesh CreateCircleMesh(float radius, int segments)
    {
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[segments + 1];
        int[] triangles = new int[segments * 3];

        vertices[0] = Vector3.zero; // centro

        for (int i = 1; i <= segments; i++)
        {
            float angle = (i - 1) * Mathf.PI * 2f / segments;
            vertices[i] = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);
        }

        for (int i = 0; i < segments; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = (i + 2 > segments) ? 1 : i + 2;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        return mesh;
    }

    // --- Setters públicos ---

    public void SetDamage(int newDamage) => damage = newDamage;
    public void SetSpeed(float newSpeed) => speed = newSpeed;

    public void SetHoming(bool active, float strength = 200f)
    {
        isHoming = active;
        homingStrength = strength;
        if (active) homingTarget = FindNearestEnemy();
    }

    public void SetHomingSoft(bool active)
    {
        SetHoming(active, 1.5f);  // ← Más suave todavía para SeekingStars
    }

    public void SetColorOverride(Color color)
    {
        hasColorOverride = true;
        projectileColor = color;
        if (sr != null) sr.color = color;
    }

    public void SetPiercing(bool active) => isPiercing = active;

    public void SetSplash(bool active, float radius = 1.5f, int splDamage = 1)
    {
        isSplash = active;
        splashRadius = radius;
        splashDamage = splDamage;
    }

    private void ReturnToPool()
    {
        if (rb != null) rb.linearVelocity = Vector2.zero;
        if (rb != null) rb.angularVelocity = 0f;
        if (ObjectPool.Instance != null)
            ObjectPool.Instance.ReturnToPool(gameObject);
        else
            gameObject.SetActive(false);
    }

    private void OnDrawGizmos()
    {
        if (isSplash)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, splashRadius);
        }
    }
}