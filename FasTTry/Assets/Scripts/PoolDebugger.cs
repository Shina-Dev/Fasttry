using UnityEngine;

public class PoolDebugger : MonoBehaviour
{
    private void Update()
    {
        // Cada 2 segundos, contar objetos
        if (Time.frameCount % 120 == 0)
        {
            CountPoolObjects();
        }
    }

    private void CountPoolObjects()
    {
        // Contar enemigos
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        int activeEnemies = 0;
        int inactiveEnemies = 0;

        foreach (GameObject enemy in enemies)
        {
            if (enemy.activeInHierarchy)
                activeEnemies++;
            else
                inactiveEnemies++;
        }

        Debug.Log($"ENEMIGOS: {activeEnemies} activos, {inactiveEnemies} inactivos (Total: {enemies.Length})");

        // Contar proyectiles
        GameObject[] projectiles = GameObject.FindGameObjectsWithTag("Projectile");
        int activeProj = 0;
        int inactiveProj = 0;

        foreach (GameObject proj in projectiles)
        {
            if (proj.activeInHierarchy)
                activeProj++;
            else
                inactiveProj++;
        }

        Debug.Log($"PROYECTILES: {activeProj} activos, {inactiveProj} inactivos (Total: {projectiles.Length})");
    }
}