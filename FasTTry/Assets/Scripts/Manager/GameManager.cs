using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Estadísticas de la partida")]  
    public int enemiesKilledThisMatch = 0;
    public int spinsUsedThisMatch = 0;

    [Header("Game State")]
    public bool isGameActive = false;  
    public float gameSpeed = 1f;

    [Header("Player Stats")]
    public int playerLives = 3;
    public int maxLives = 5;

    [Header("Score")]
    public int currentScore = 0;
    public float distanceTraveled = 0f;

    [Header("Difficulty")]
    public float difficultyMultiplier = 1f;
    public float distancePerDifficultyIncrease = 100f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

           
            Screen.SetResolution(1080, 1920, FullScreenMode.Windowed);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        
        Debug.Log("GameManager listo. Esperando a que presiones Play...");
    }

    private void Update()
    {
        if (!isGameActive) return;  // ← Si el juego NO está activo, no hace nada

        // Actualizar distancia
        distanceTraveled += gameSpeed * Time.deltaTime * 10f;

        // Aumentar dificultad cada X metros
        UpdateDifficulty();
    }

    public void StartGame()
    {
        Debug.Log("🎮 ¡JUEGO INICIADO!");

        isGameActive = true;
        playerLives = 3;
        currentScore = 0;
        distanceTraveled = 0f;
        difficultyMultiplier = 1f;
        Time.timeScale = 1f;
        enemiesKilledThisMatch = 0;
        spinsUsedThisMatch = 0;

        // Resetear otros managers
        if (SlotMachine.Instance != null)
        {
            SlotMachine.Instance.ResetSlotMachine();
        }

        if (WeaponManager.Instance != null)
        {
            WeaponManager.Instance.ResetWeaponManager();
        }

       
        // Limpiar enemigos de la escena pa evitar bugs
        LimpiarEnemigos();
    }

    private void LimpiarEnemigos()
    {
        // Buscar todos los enemigos activos y desactivarlos
        GameObject[] enemigos = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemigo in enemigos)
        {
            if (enemigo.activeInHierarchy)
            {
                enemigo.SetActive(false);
            }
        }

        // Buscar todos los proyectiles enemigos
        GameObject[] proyectiles = GameObject.FindGameObjectsWithTag("EnemyProjectile");
        foreach (GameObject proyectil in proyectiles)
        {
            if (proyectil.activeInHierarchy)
            {
                proyectil.SetActive(false);
            }
        }

        // Buscar proyectiles del jugador
        GameObject[] proyectilesJugador = GameObject.FindGameObjectsWithTag("Projectile");
        foreach (GameObject proyectil in proyectilesJugador)
        {
            if (proyectil.activeInHierarchy)
            {
                proyectil.SetActive(false);
            }
        }

        Debug.Log("🧹 Escena limpiada - Enemigos y proyectiles removidos");
    }

    public void GameOver()
    {
        isGameActive = false;
        Time.timeScale = 0f;

        // ← ARREGLADO: Convertir correctamente a KM
        // distanceTraveled está en "unidades de juego"
        // Para convertir a metros reales, multiplicamos (ajusta el multiplicador según tu juego)
        float distanciaMetros = distanceTraveled;  // ← Ajusta este número si es necesario
        float distanciaKM = distanciaMetros / 1000f;

        Debug.Log($"=== GAME OVER ===");
        Debug.Log($"📏 Distancia: {distanciaKM:F2} km ({distanciaMetros:F0} m)");
        Debug.Log($"💀 Enemigos: {enemiesKilledThisMatch}");
        Debug.Log($"🎰 Spins: {spinsUsedThisMatch}");

        // Guardar estadísticas
        if (StatsDatabase.Instance != null)
        {
            StatsDatabase.Instance.GuardarPartida(
                distanciaKM,
                enemiesKilledThisMatch,
                spinsUsedThisMatch
            );
        }

        // Mostrar pantalla de Game Over
        if (MenusControles.Instance != null)
        {
            MenusControles.Instance.MostrarGameOver();
        }
        else
        {
            Debug.LogError("MenusControles no encontrado");
        }
    }
    public void PlayerTakeDamage(int damage = 1)
    {
        // ✨ VERIFICAR INVENCIBILIDAD PRIMERO
        if (WeaponManager.Instance != null && WeaponManager.Instance.IsInvincible())
        {
            Debug.Log("DAÑO BLOQUEADO POR ESCUDO DIAMANTE");
            return;  // No recibe daño
        }

        playerLives -= damage;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPlayerHit();
        }

        Debug.Log($"Player hit! Lives remaining: {playerLives}");

        if (playerLives <= 0)
        {
            GameOver();
        }
    }

    public void AddScore(int points)
    {
        currentScore += points;
        enemiesKilledThisMatch++;
    }

    public void RegisterSpinUsed()
    {
        spinsUsedThisMatch++;
        Debug.Log($"Spins usados esta partida: {spinsUsedThisMatch}");
    }

    private void UpdateDifficulty()
    {
        // Cada 100m aumenta la dificultad un 10%
        float newMultiplier = 1f + (distanceTraveled / distancePerDifficultyIncrease) * 0.1f;
        difficultyMultiplier = Mathf.Min(newMultiplier, 3f); // Cap en 3x
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;

        Debug.Log("Reiniciando juego...");

        StartGame();
    }

    public void PauseGame()
    {
        isGameActive = false;
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        isGameActive = true;
        Time.timeScale = 1f;
    }
}