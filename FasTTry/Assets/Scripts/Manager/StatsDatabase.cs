using UnityEngine;

public class StatsDatabase : MonoBehaviour
{
    public static StatsDatabase Instance { get; private set; }

    [Header("Estadísticas")]
    public float recordKM = 0f;
    public float lastMatchKM = 0f;
    public int lastMatchEnemies = 0;
    public int lastMatchSpins = 0;

    // Claves para PlayerPrefs
    private const string KEY_RECORD = "record_km";
    private const string KEY_LAST_KM = "last_match_km";
    private const string KEY_LAST_ENEMIES = "last_match_enemies";
    private const string KEY_LAST_SPINS = "last_match_spins";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        CargarEstadisticas();
    }

    public void CargarEstadisticas()
    {
        recordKM = PlayerPrefs.GetFloat(KEY_RECORD, 0f);
        lastMatchKM = PlayerPrefs.GetFloat(KEY_LAST_KM, 0f);
        lastMatchEnemies = PlayerPrefs.GetInt(KEY_LAST_ENEMIES, 0);
        lastMatchSpins = PlayerPrefs.GetInt(KEY_LAST_SPINS, 0);

        Debug.Log($"✅ Stats cargadas - Récord: {recordKM:F2} km | Última: {lastMatchKM:F2} km");
    }

    public void GuardarPartida(float distanciaKM, int enemigosDestruidos, int spinsUsados)
    {
        lastMatchKM = distanciaKM;
        lastMatchEnemies = enemigosDestruidos;
        lastMatchSpins = spinsUsados;

        bool esNuevoRecord = distanciaKM > recordKM;
        if (esNuevoRecord)
        {
            recordKM = distanciaKM;
            Debug.Log($"🏆 ¡NUEVO RÉCORD! {recordKM:F2} km");
        }

        PlayerPrefs.SetFloat(KEY_RECORD, recordKM);
        PlayerPrefs.SetFloat(KEY_LAST_KM, lastMatchKM);
        PlayerPrefs.SetInt(KEY_LAST_ENEMIES, lastMatchEnemies);
        PlayerPrefs.SetInt(KEY_LAST_SPINS, lastMatchSpins);
        PlayerPrefs.Save(); // Forzar escritura inmediata

        Debug.Log($"💾 Partida guardada - {distanciaKM:F2} km | {enemigosDestruidos} enemigos | {spinsUsados} spins");
    }

    public void ResetearEstadisticas()
    {
        recordKM = 0f;
        lastMatchKM = 0f;
        lastMatchEnemies = 0;
        lastMatchSpins = 0;

        PlayerPrefs.SetFloat(KEY_RECORD, 0f);
        PlayerPrefs.SetFloat(KEY_LAST_KM, 0f);
        PlayerPrefs.SetInt(KEY_LAST_ENEMIES, 0);
        PlayerPrefs.SetInt(KEY_LAST_SPINS, 0);
        PlayerPrefs.Save();

        Debug.Log("🔄 Estadísticas reseteadas");
    }

    // Getters públicos para UI
    public float GetRecordKM() => recordKM;
    public float GetLastMatchKM() => lastMatchKM;
    public int GetLastMatchEnemies() => lastMatchEnemies;
    public int GetLastMatchSpins() => lastMatchSpins;
}