using UnityEngine;
using System.Collections.Generic;

public class SpawnManager : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private float baseSpawnInterval = 3f;
    [SerializeField] private float spawnYPosition = 7f;
    [SerializeField] private float minX = -2.25f;
    [SerializeField] private float maxX = 2.25f;

    [Header("Lane Settings")]
    [SerializeField] private int totalLanes = 4;
    [SerializeField] private float laneWidth = 1.5f;

    [Header("Difficulty Scaling")]
    [SerializeField] private float minSpawnInterval = 1f;
    [SerializeField] private float easySpeedMultiplier = 1f;      // 0-300m
    [SerializeField] private float mediumSpeedMultiplier = 1.3f;  // 300-700m
    [SerializeField] private float hardSpeedMultiplier = 1.5f;    // 700m-1.3km
    [SerializeField] private float extremeSpeedMultiplier = 1.8f; // 1.3km+

    private float nextSpawnTime = 0f;
    private bool canSpawn = true;
    private float[] lanePositions;
    private bool[] laneOccupied;
    private string currentDifficulty = "EASY";

    private void Start()
    {
        CalculateLanePositions();
        laneOccupied = new bool[totalLanes];
        nextSpawnTime = Time.time + 2f;

        Debug.Log("SpawnManager inicializado con sistema de dificultad progresiva");
    }

    private void Update()
    {
        if (GameManager.Instance == null) return;
        if (!GameManager.Instance.isGameActive) return;
        if (!canSpawn) return;

        // Actualizar dificultad actual
        UpdateDifficulty();

        // Actualizar estado de carriles
        UpdateLaneOccupancy();

        // Spawn de enemigos con intervalo ajustado por dificultad
        if (Time.time >= nextSpawnTime)
        {
            SpawnEnemy();

            // Ajustar intervalo según dificultad
            float adjustedInterval = GetAdjustedSpawnInterval();
            nextSpawnTime = Time.time + adjustedInterval;
        }
    }

    private void UpdateDifficulty()
    {
        if (GameManager.Instance == null) return;

        // ← distanceTraveled está en "unidades de juego", no metros
        float distance = GameManager.Instance.distanceTraveled;

        string newDifficulty;
        if (distance < 300f)        // 300 unidades = ~300m en el juego
            newDifficulty = "EASY";
        else if (distance < 700f)   // 700 unidades = ~700m
            newDifficulty = "MEDIUM";
        else if (distance < 1300f)  // 1300 unidades = ~1.3km
            newDifficulty = "HARD";
        else                        // 1300+ unidades = 1.3km+
            newDifficulty = "EXTREME";

        if (newDifficulty != currentDifficulty)
        {
            currentDifficulty = newDifficulty;
            float km = distance / 1000f;
            Debug.Log($"🔥 DIFICULTAD CAMBIADA A: {currentDifficulty} | Distancia: {distance:F0} unidades ({km:F2} km en UI)");
        }
    }

    private float GetAdjustedSpawnInterval()
    {
        float speedMultiplier = GetSpeedMultiplier();

        // A mayor velocidad, menor intervalo de spawn
        float adjustedInterval = baseSpawnInterval / speedMultiplier;
        return Mathf.Max(adjustedInterval, minSpawnInterval);
    }

    private float GetSpeedMultiplier()
    {
        switch (currentDifficulty)
        {
            case "EASY": return easySpeedMultiplier;
            case "MEDIUM": return mediumSpeedMultiplier;
            case "HARD": return hardSpeedMultiplier;
            case "EXTREME": return extremeSpeedMultiplier;
            default: return 1f;
        }
    }

    private void UpdateLaneOccupancy()
    {
        for (int i = 0; i < laneOccupied.Length; i++)
        {
            laneOccupied[i] = false;
        }

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject enemy in enemies)
        {
            if (!enemy.activeInHierarchy) continue;

            float enemyX = enemy.transform.position.x;
            int laneIndex = GetClosestLaneIndex(enemyX);

            if (laneIndex >= 0 && laneIndex < totalLanes)
            {
                laneOccupied[laneIndex] = true;
            }
        }
    }

    private int GetClosestLaneIndex(float xPosition)
    {
        int closestIndex = 0;
        float closestDistance = Mathf.Abs(lanePositions[0] - xPosition);

        for (int i = 1; i < lanePositions.Length; i++)
        {
            float distance = Mathf.Abs(lanePositions[i] - xPosition);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestIndex = i;
            }
        }

        return closestIndex;
    }

    private void SpawnEnemy()
    {
        if (ObjectPool.Instance == null)
        {
            Debug.LogWarning("ObjectPool no encontrado!");
            return;
        }

        // Obtener carriles disponibles
        List<int> availableLanes = new List<int>();
        for (int i = 0; i < totalLanes; i++)
        {
            if (!laneOccupied[i])
            {
                availableLanes.Add(i);
            }
        }

        if (availableLanes.Count == 0)
        {
            return;
        }

        // Elegir carril aleatorio
        int randomLaneIndex = availableLanes[Random.Range(0, availableLanes.Count)];
        float spawnX = lanePositions[randomLaneIndex];
        Vector3 spawnPosition = new Vector3(spawnX, spawnYPosition, 0);

        // Spawnear enemigo
        GameObject enemyObj = ObjectPool.Instance.SpawnFromPool("Enemy", spawnPosition, Quaternion.identity);

        if (enemyObj != null)
        {
            laneOccupied[randomLaneIndex] = true;

            Enemy enemyScript = enemyObj.GetComponent<Enemy>();
            if (enemyScript != null)
            {
                ConfigureEnemyByDifficulty(enemyScript);
            }
        }
    }

    private void ConfigureEnemyByDifficulty(Enemy enemy)
    {
        Enemy.EnemyType type;

        switch (currentDifficulty)
        {
            case "EASY":
                // Solo enemigos básicos
                type = Random.value < 0.7f ? Enemy.EnemyType.Shooter : Enemy.EnemyType.FastShooter;
                break;

            case "MEDIUM":
                // Mezcla de básicos, sin Snipers aún
                float randMedium = Random.value;
                if (randMedium < 0.5f)
                    type = Enemy.EnemyType.Shooter;
                else if (randMedium < 0.8f)
                    type = Enemy.EnemyType.FastShooter;
                else
                    type = Enemy.EnemyType.HeavyShooter;
                break;

            case "HARD":
                // ← 50% básicos + 50% Snipers
                float randHard = Random.value;
                if (randHard < 0.25f)
                    type = Enemy.EnemyType.Shooter;
                else if (randHard < 0.4f)
                    type = Enemy.EnemyType.FastShooter;
                else if (randHard < 0.5f)
                    type = Enemy.EnemyType.HeavyShooter;
                else
                    type = Enemy.EnemyType.Sniper;  // ← 50% Snipers
                break;

            case "EXTREME":
                // ← 100% Snipers
                type = Enemy.EnemyType.Sniper;
                break;

            default:
                type = Enemy.EnemyType.Shooter;
                break;
        }

        enemy.SetEnemyType(type);

        // Ajustar posición objetivo según dificultad
        float targetY = Mathf.Lerp(4.5f, 3f, GetSpeedMultiplier() / extremeSpeedMultiplier);
        enemy.SetTargetPosition(targetY);

        if (type == Enemy.EnemyType.Sniper)
        {
            Debug.Log($"🎯 SNIPER spawneado | Dificultad: {currentDifficulty}");
        }
    }

    private void CalculateLanePositions()
    {
        lanePositions = new float[totalLanes];

        float totalWidth = (totalLanes - 1) * laneWidth;
        float startX = -totalWidth / 2f;

        for (int i = 0; i < totalLanes; i++)
        {
            lanePositions[i] = startX + i * laneWidth;
        }

        minX = lanePositions[0];
        maxX = lanePositions[totalLanes - 1];
    }

    public void SetCanSpawn(bool state)
    {
        canSpawn = state;
    }

    private void OnDrawGizmos()
    {
        if (lanePositions == null || lanePositions.Length == 0) return;

        // Línea de spawn
        Gizmos.color = Color.yellow;
        Vector3 leftPoint = new Vector3(minX, spawnYPosition, 0);
        Vector3 rightPoint = new Vector3(maxX, spawnYPosition, 0);
        Gizmos.DrawLine(leftPoint, rightPoint);

        // Carriles
        for (int i = 0; i < lanePositions.Length; i++)
        {
            Vector3 lanePos = new Vector3(lanePositions[i], spawnYPosition, 0);

            if (Application.isPlaying && laneOccupied != null && i < laneOccupied.Length)
            {
                Gizmos.color = laneOccupied[i] ? Color.red : Color.green;
            }
            else
            {
                Gizmos.color = Color.white;
            }

            Gizmos.DrawWireSphere(lanePos, 0.3f);

            Gizmos.color = Color.gray;
            Gizmos.DrawLine(new Vector3(lanePositions[i], -10, 0), new Vector3(lanePositions[i], 10, 0));
        }
    }
}