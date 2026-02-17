using UnityEngine;
using Mono.Data.Sqlite;
using System;

public class StatsDatabase : MonoBehaviour
{
    public static StatsDatabase Instance { get; private set; }

    private string rutaBaseDatos
    {
        get { return "URI=file:" + Application.persistentDataPath + "/FasttryStats.db"; }
    }

    [Header("Estadísticas")]
    public float recordKM = 0f;
    public float lastMatchKM = 0f;
    public int lastMatchEnemies = 0;
    public int lastMatchSpins = 0;

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

        CrearTablaEstadisticas();
        CargarEstadisticas();
    }

    private void CrearTablaEstadisticas()
    {
        SqliteConnection conexion = null;
        try
        {
            conexion = new SqliteConnection(rutaBaseDatos);
            conexion.Open();

            SqliteCommand comando = conexion.CreateCommand();
            comando.CommandText = @"
                CREATE TABLE IF NOT EXISTS GameStats (
                    id INTEGER PRIMARY KEY CHECK (id = 1),
                    record_km REAL DEFAULT 0,
                    last_match_km REAL DEFAULT 0,
                    last_match_enemies INTEGER DEFAULT 0,
                    last_match_spins INTEGER DEFAULT 0
                )";
            comando.ExecuteNonQuery();

            Debug.Log("? Tabla de estadísticas creada");

            comando.CommandText = "SELECT COUNT(*) FROM GameStats";
            long count = (long)comando.ExecuteScalar();

            if (count == 0)
            {
                comando.CommandText = @"
                    INSERT INTO GameStats (id, record_km, last_match_km, last_match_enemies, last_match_spins) 
                    VALUES (1, 0, 0, 0, 0)";
                comando.ExecuteNonQuery();
                Debug.Log("? Registro inicial creado");
            }
        }
        catch (Exception error)
        {
            Debug.LogError("? Error al crear tabla: " + error.Message);
        }
        finally
        {
            if (conexion != null && conexion.State == System.Data.ConnectionState.Open)
                conexion.Close();
        }
    }

    public void CargarEstadisticas()
    {
        SqliteConnection conexion = null;
        try
        {
            conexion = new SqliteConnection(rutaBaseDatos);
            conexion.Open();

            SqliteCommand comando = conexion.CreateCommand();
            comando.CommandText = "SELECT record_km, last_match_km, last_match_enemies, last_match_spins FROM GameStats WHERE id = 1";

            SqliteDataReader lector = comando.ExecuteReader();

            if (lector.Read())
            {
                recordKM = Convert.ToSingle(lector["record_km"]);
                lastMatchKM = Convert.ToSingle(lector["last_match_km"]);
                lastMatchEnemies = Convert.ToInt32(lector["last_match_enemies"]);
                lastMatchSpins = Convert.ToInt32(lector["last_match_spins"]);

                Debug.Log($"?? Stats cargadas - Récord: {recordKM:F2} km | Última: {lastMatchKM:F2} km");
            }

            lector.Close();
        }
        catch (Exception error)
        {
            Debug.LogError("? Error al cargar estadísticas: " + error.Message);
        }
        finally
        {
            if (conexion != null && conexion.State == System.Data.ConnectionState.Open)
                conexion.Close();
        }
    }

    public void GuardarPartida(float distanciaKM, int enemigosDestruidos, int spinsUsados)
    {
        SqliteConnection conexion = null;
        try
        {
            conexion = new SqliteConnection(rutaBaseDatos);
            conexion.Open();

            SqliteCommand comando = conexion.CreateCommand();

            lastMatchKM = distanciaKM;
            lastMatchEnemies = enemigosDestruidos;
            lastMatchSpins = spinsUsados;

            bool esNuevoRecord = distanciaKM > recordKM;
            if (esNuevoRecord)
            {
                recordKM = distanciaKM;
                Debug.Log($"?? ¡NUEVO RÉCORD! {recordKM:F2} km");
            }

            comando.CommandText = @"
                UPDATE GameStats 
                SET record_km = @record_km,
                    last_match_km = @last_km,
                    last_match_enemies = @last_enemies,
                    last_match_spins = @last_spins
                WHERE id = 1";

            comando.Parameters.AddWithValue("@record_km", recordKM);
            comando.Parameters.AddWithValue("@last_km", lastMatchKM);
            comando.Parameters.AddWithValue("@last_enemies", lastMatchEnemies);
            comando.Parameters.AddWithValue("@last_spins", lastMatchSpins);
            comando.ExecuteNonQuery();

            Debug.Log($"?? Partida guardada - {distanciaKM:F2} km | {enemigosDestruidos} enemigos | {spinsUsados} spins");
        }
        catch (Exception error)
        {
            Debug.LogError("? Error al guardar: " + error.Message);
        }
        finally
        {
            if (conexion != null && conexion.State == System.Data.ConnectionState.Open)
                conexion.Close();
        }
    }

    public void ResetearEstadisticas()
    {
        SqliteConnection conexion = null;
        try
        {
            conexion = new SqliteConnection(rutaBaseDatos);
            conexion.Open();

            SqliteCommand comando = conexion.CreateCommand();
            comando.CommandText = @"
                UPDATE GameStats 
                SET record_km = 0,
                    last_match_km = 0,
                    last_match_enemies = 0,
                    last_match_spins = 0
                WHERE id = 1";
            comando.ExecuteNonQuery();

            recordKM = 0f;
            lastMatchKM = 0f;
            lastMatchEnemies = 0;
            lastMatchSpins = 0;

            Debug.Log("?? Estadísticas reseteadas");
        }
        catch (Exception error)
        {
            Debug.LogError("? Error al resetear: " + error.Message);
        }
        finally
        {
            if (conexion != null && conexion.State == System.Data.ConnectionState.Open)
                conexion.Close();
        }
    }

    // Getters públicos para UI
    public float GetRecordKM() => recordKM;
    public float GetLastMatchKM() => lastMatchKM;
    public int GetLastMatchEnemies() => lastMatchEnemies;
    public int GetLastMatchSpins() => lastMatchSpins;
}