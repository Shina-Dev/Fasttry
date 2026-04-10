using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float laneWidth = 1.5f;
    [SerializeField] private int totalLanes = 4;

    [Header("Movement Type")]
    [SerializeField] private bool useLaneSystem = true;

    [Header("Shield Effect")]  
    [SerializeField] private float shieldPulseSpeed = 4f;
    [SerializeField] private float shieldMinAlpha = 0.3f;
    [SerializeField] private float shieldMaxAlpha = 1f;

    private int currentLane = 1;
    private float targetXPosition;
    private float minX, maxX;
    private float horizontalInput;
    private bool canMove = true;

    // Variables para efecto de escudo
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool wasInvincible = false;
    private float shieldTimer = 0f;
    private float touchStartX;
    private float touchStartY;
    private float lastMoveTime = 0f;
    private const float MOVE_COOLDOWN = 0.3f;

    private void Start()
    {

        // Obtener SpriteRenderer
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        CalculateBoundaries();

        if (useLaneSystem)
        {
            targetXPosition = GetLanePosition(currentLane);
            transform.position = new Vector3(targetXPosition, transform.position.y, 0);
        }
    }

    private void Update()
    {
        if (GameManager.Instance == null) return;
        if (!GameManager.Instance.isGameActive || !canMove) return;

        HandleInput();
        HandleMovement();
        UpdateShieldEffect();  
    }

   
    private void UpdateShieldEffect()
    {
        if (WeaponManager.Instance == null || spriteRenderer == null) return;

        bool isInvincible = WeaponManager.Instance.IsInvincible();

        // Activar efecto cuando se activa el escudo
        if (isInvincible && !wasInvincible)
        {
            Debug.Log("💎 Efecto visual de escudo activado");
            wasInvincible = true;
            shieldTimer = 0f;
        }

        // Desactivar efecto cuando expira
        if (!isInvincible && wasInvincible)
        {
            Debug.Log("💎 Efecto visual de escudo desactivado");
            wasInvincible = false;
            spriteRenderer.color = originalColor;
        }

        // Animar transparencia mientras está activo
        if (isInvincible)
        {
            shieldTimer += Time.deltaTime * shieldPulseSpeed;

            // Ping-pong entre min y max alpha
            float alpha = Mathf.Lerp(shieldMinAlpha, shieldMaxAlpha, Mathf.PingPong(shieldTimer, 1f));

            // Aplicar color con tinte cyan
            Color shieldColor = Color.Lerp(originalColor, Color.cyan, 0.3f);
            shieldColor.a = alpha;
            spriteRenderer.color = shieldColor;
        }
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            MoveLane(-1);
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            MoveLane(1);

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.position.y < Screen.height * 0.20f) return;

            if (touch.phase == TouchPhase.Began)
            {
                touchStartX = touch.position.x;
                touchStartY = touch.position.y;
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                float swipeDelta = touch.position.x - touchStartX;

                if (Mathf.Abs(swipeDelta) > 80f && Time.time > lastMoveTime + MOVE_COOLDOWN)
                {
                    if (swipeDelta > 0)
                        MoveLane(1);
                    else
                        MoveLane(-1);

                    lastMoveTime = Time.time;
                    touchStartX = touch.position.x;
                }
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                float swipeDelta = touch.position.x - touchStartX;

                // Si fue un tap (poco movimiento)
                if (Mathf.Abs(swipeDelta) < 20f)
                {
                    if (touch.position.x < Screen.width / 2)
                        MoveLane(-1);
                    else
                        MoveLane(1);
                }
            }
        }
    }

    private void HandleMovement()
    {
        if (useLaneSystem)
        {
            float newX = Mathf.Lerp(transform.position.x, targetXPosition, moveSpeed * Time.deltaTime);
            transform.position = new Vector3(newX, transform.position.y, transform.position.z);
        }
        else
        {
            float newX = transform.position.x + horizontalInput * moveSpeed * Time.deltaTime;
            newX = Mathf.Clamp(newX, minX, maxX);
            transform.position = new Vector3(newX, transform.position.y, transform.position.z);
        }
    }

    private void MoveLane(int direction)
    {
        int newLane = currentLane + direction;

        if (newLane >= 0 && newLane < totalLanes)
        {
            currentLane = newLane;
            targetXPosition = GetLanePosition(currentLane);
        }
    }

    private float GetLanePosition(int laneIndex)
    {
        float totalWidth = (totalLanes - 1) * laneWidth;
        return -totalWidth / 2f + laneIndex * laneWidth;
    }

    private void CalculateBoundaries()
    {
        if (useLaneSystem)
        {
            minX = GetLanePosition(0);
            maxX = GetLanePosition(totalLanes - 1);
        }
        else
        {
            float totalWidth = (totalLanes - 1) * laneWidth;
            minX = -totalWidth / 2f;
            maxX = totalWidth / 2f;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            HandleEnemyCollision(collision.gameObject);
        }

        if (collision.CompareTag("Obstacle"))
        {
            HandleObstacleCollision();
        }

        if (collision.CompareTag("PowerUp"))
        {
            HandlePowerUp(collision.gameObject);
        }
    }

    private void HandleEnemyCollision(GameObject enemy)
    {
        Debug.Log("Collided with enemy!");
        GameManager.Instance.PlayerTakeDamage(1);
        Destroy(enemy);
    }

    private void HandleObstacleCollision()
    {
        Debug.Log("Hit obstacle!");
        GameManager.Instance.PlayerTakeDamage(1);
    }

    private void HandlePowerUp(GameObject powerUp)
    {
        Destroy(powerUp);
    }

    public void SetCanMove(bool state)
    {
        canMove = state;
    }

    public int GetCurrentLane()
    {
        return currentLane;
    }

    private void OnDrawGizmos()
    {
        if (!useLaneSystem) return;

        Gizmos.color = Color.yellow;
        for (int i = 0; i < totalLanes; i++)
        {
            float xPos = GetLanePosition(i);
            Gizmos.DrawLine(new Vector3(xPos, -10, 0), new Vector3(xPos, 10, 0));
        }
    }
}