using UnityEngine;
using TMPro;
using System.Collections;

public class MenusControles : MonoBehaviour
{
    public static MenusControles Instance { get; private set; }

    [Header("Panels")]
    public GameObject menuPanel;        // Panel del menú principal
    public GameObject gamePanel;        // Panel del juego (HUD)
    public GameObject gameOverPanel;    // Panel de Game Over completo
    public GameObject configPanel;


    [Header("Menu Principal")]
    public TextMeshProUGUI textRecordInicio;  // ← Texto del récord en el menú
  
    [Header("Config Panel")]
    public UnityEngine.UI.Slider musicSlider;
    public UnityEngine.UI.Slider sfxSlider;

    [Header("Game Over - Fase 1: Texto rojo")]
    public GameObject textGameOver;     // "GAME OVER" en rojo

    [Header("Game Over - Fase 2: Estadísticas")]
    public GameObject panelEstadisticas;           // Panel con estadísticas
    public TextMeshProUGUI textRecord;             // Récord histórico
    public TextMeshProUGUI textDistancia;          // Esta partida
    public TextMeshProUGUI textEnemigos;           // Esta partida
    public TextMeshProUGUI textSpins;              // Esta partida
    public GameObject textNewRecord;               

    [Header("Configuración")]
    public float tiempoGameOver = 2f;   // Tiempo antes de mostrar estadísticas

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

    private void Start()
    {
        // Al iniciar: mostrar menú y ocultar todo lo demás
        menuPanel.SetActive(true);
        gamePanel.SetActive(false);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        // Asegurarse de que el juego NO esté activo
        if (GameManager.Instance != null)
        {
            GameManager.Instance.isGameActive = false;
        }

        // ← AGREGAR: Reproducir música del menú
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMenuMusic();
        }

        // Actualizar récord en el menú
        ActualizarRecordInicio();

        Debug.Log("Menú mostrado. Esperando botón Play...");
    }

    /// <summary>
    /// Actualiza el texto del récord en el menú de inicio
    /// </summary>
    private void ActualizarRecordInicio()
    {
        if (textRecordInicio != null)
        {
            if (StatsDatabase.Instance != null)
            {
                float record = StatsDatabase.Instance.GetRecordKM();
                textRecordInicio.text = $"{record:F2} KM";

                Debug.Log($"🏆 Récord mostrado en menú: {record:F2} KM");
            }
            else
            {
                Debug.LogWarning("StatsDatabase no encontrado al actualizar récord del menú");
                textRecordInicio.text = "RÉCORD: 0.00 KM";
            }
        }
        else
        {
            Debug.LogWarning("textRecordInicio no está asignado en el Inspector");
        }
    }

    /// <summary>
    /// Inicia el juego (llamado desde el botón Play)
    /// </summary>
    public void StartGame()
    {
        Debug.Log("Botón Play presionado");

        // ← AGREGAR: Sonido de botón
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }

        // Ocultar menú y game over
        menuPanel.SetActive(false);
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        // Mostrar HUD del juego
        gamePanel.SetActive(true);

        // ← AGREGAR: Cambiar a música del juego
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayGameplayMusic();
        }

        // Iniciar el juego
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGame();
        }
        else
        {
            Debug.LogError("GameManager no encontrado");
        }
    }

    /// <summary>
    /// Muestra el Game Over con animación de 2 fases
    /// </summary>
    public void MostrarGameOver()
    {
        StartCoroutine(SecuenciaGameOver());
    }

    private IEnumerator SecuenciaGameOver()
    {
        // ← AGREGAR: Sonido de Game Over
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayGameOverSound();
        }

        // Ocultar HUD del juego
        if (gamePanel != null)
            gamePanel.SetActive(false);

        // Activar panel de Game Over
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        // FASE 1: Mostrar "GAME OVER" en rojo
        if (textGameOver != null)
            textGameOver.SetActive(true);

        if (panelEstadisticas != null)
            panelEstadisticas.SetActive(false);

        Debug.Log("GAME OVER mostrado");

        // Esperar 2 segundos (usa Realtime porque Time.timeScale = 0)
        yield return new WaitForSecondsRealtime(tiempoGameOver);

        // FASE 2: Ocultar "GAME OVER" y mostrar estadísticas
        if (textGameOver != null)
            textGameOver.SetActive(false);

        if (panelEstadisticas != null)
            panelEstadisticas.SetActive(true);

        ActualizarEstadisticas();
    }

    /// <summary>
    /// Actualiza los textos con las estadísticas
    /// </summary>
    private void ActualizarEstadisticas()
    {
        if (StatsDatabase.Instance == null)
        {
            Debug.LogWarning("StatsDatabase no encontrado");
            return;
        }

        // Obtener datos
        float recordKM = StatsDatabase.Instance.GetRecordKM();
        float lastKM = StatsDatabase.Instance.GetLastMatchKM();
        int lastEnemies = StatsDatabase.Instance.GetLastMatchEnemies();
        int lastSpins = StatsDatabase.Instance.GetLastMatchSpins();

        // Actualizar textos
        if (textRecord != null)
            textRecord.text = $"{recordKM:F2} KM";

        if (textDistancia != null)
            textDistancia.text = $"{lastKM:F2} KM";

        if (textEnemigos != null)
            textEnemigos.text = lastEnemies.ToString();

        if (textSpins != null)
            textSpins.text = lastSpins.ToString();

        // Mostrar "NEW RECORD!" si corresponde
        bool esNuevoRecord = lastKM >= recordKM && lastKM > 0;
        if (textNewRecord != null)
        {
            textNewRecord.SetActive(esNuevoRecord);
        }

        Debug.Log($"📊 Estadísticas mostradas:");
        Debug.Log($"   🏆 Récord: {recordKM:F2} km");
        Debug.Log($"   📏 Esta partida: {lastKM:F2} km");
        Debug.Log($"   💀 Enemigos: {lastEnemies}");
        Debug.Log($"   🎰 Spins: {lastSpins}");
    }

    /// <summary>
    /// Reinicia el juego (llamado desde botón Reintentar)
    /// </summary>
    public void Reintentar()
    {
        Debug.Log("Reintentar presionado");

        // ← AGREGAR: Sonido de botón
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
            AudioManager.Instance.PlayGameplayMusic(); // Volver a música del juego
        }

        Time.timeScale = 1f;

        // Ocultar panels
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        menuPanel.SetActive(false);
        gamePanel.SetActive(true);

        // Reiniciar juego (SIN recargar escena)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGame();
        }
    }

    /// <summary>
    /// Vuelve al menú principal (llamado desde botón Menu)
    /// </summary>
    public void VolverAlMenu()
    {
        Debug.Log("Volver al menú");

        // ← AGREGAR: Detener coroutines de Game Over
        StopAllCoroutines();

        // Sonido de botón y música del menú
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
            AudioManager.Instance.PlayMenuMusic();
        }

        // Reanudar el tiempo
        Time.timeScale = 1f;

        // Mostrar menú, ocultar todo lo demás
        menuPanel.SetActive(true);
        gamePanel.SetActive(false);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        // Resetear managers
        if (GameManager.Instance != null)
        {
            GameManager.Instance.isGameActive = false;

            if (SlotMachine.Instance != null)
                SlotMachine.Instance.ResetSlotMachine();

            if (WeaponManager.Instance != null)
                WeaponManager.Instance.ResetWeaponManager();
        }

        // Actualizar récord al volver al menú
        ActualizarRecordInicio();

        Debug.Log("Vuelto al menú - Todo reseteado");
    }
    public void AbrirConfig()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();

        if (configPanel != null)
            configPanel.SetActive(true);

        if (AudioManager.Instance != null)
        {
            // Desuscribir temporalmente para no triggerear el evento al setear valor
            if (musicSlider != null)
            {
                musicSlider.onValueChanged.RemoveAllListeners();
                musicSlider.value = AudioManager.Instance.musicVolume;
                musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            }
            if (sfxSlider != null)
            {
                sfxSlider.onValueChanged.RemoveAllListeners();
                sfxSlider.value = AudioManager.Instance.sfxVolume;
                sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            }
        }
    }
    public void CerrarConfig()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();

        if (musicSlider != null)
            musicSlider.onValueChanged.RemoveAllListeners();
        if (sfxSlider != null)
            sfxSlider.onValueChanged.RemoveAllListeners();

        if (configPanel != null)
            configPanel.SetActive(false);
    }

    public void OnMusicVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetMusicVolume(value);
    }

    public void OnSFXVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetSFXVolume(value);
    }
}