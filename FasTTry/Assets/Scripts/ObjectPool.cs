using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }

    public static ObjectPool Instance { get; private set; }

    [Header("Pools Configuration")]
    public List<Pool> pools;

    private Dictionary<string, Queue<GameObject>> poolDictionary;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (Pool poolConfig in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < poolConfig.size; i++)
            {
                GameObject obj = Instantiate(poolConfig.prefab);
                obj.SetActive(false);
                obj.transform.SetParent(transform);
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(poolConfig.tag, objectPool);
        }

        Debug.Log($"Object Pool inicializado con {pools.Count} pools");
    }

    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"Pool con tag '{tag}' no existe!");
            return null;
        }

        // Buscar objeto inactivo disponible
        GameObject objectToSpawn = null;
        int attempts = 0;
        int maxAttempts = poolDictionary[tag].Count;

        while (attempts < maxAttempts)
        {
            GameObject candidateObj = poolDictionary[tag].Dequeue();
            poolDictionary[tag].Enqueue(candidateObj);

            // Verificar que el objeto exista y esté inactivo
            if (candidateObj != null && !candidateObj.activeInHierarchy)
            {
                objectToSpawn = candidateObj;
                break;
            }

            attempts++;
        }

        // Si no encontramos ninguno disponible, CREAR UNO NUEVO
        if (objectToSpawn == null)
        {
            Debug.LogWarning($"Pool '{tag}' lleno, creando objeto adicional...");

            // Buscar el prefab original
            Pool originalPool = pools.Find(p => p.tag == tag);
            if (originalPool != null && originalPool.prefab != null)
            {
                objectToSpawn = Instantiate(originalPool.prefab);
                objectToSpawn.SetActive(false);
                objectToSpawn.transform.SetParent(transform);
                poolDictionary[tag].Enqueue(objectToSpawn);
            }
            else
            {
                Debug.LogError($"No se pudo crear objeto adicional para pool '{tag}'");
                return null;
            }
        }

        // Verificación de seguridad
        if (objectToSpawn == null)
        {
            Debug.LogError($"No se pudo obtener objeto del pool '{tag}'");
            return null;
        }

        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;
        objectToSpawn.SetActive(true);

        IPooledObject pooledObj = objectToSpawn.GetComponent<IPooledObject>();
        if (pooledObj != null)
        {
            pooledObj.OnObjectSpawn();
        }

        return objectToSpawn;
    }

    public void ReturnToPool(GameObject objectToReturn)
    {
        if (objectToReturn != null)
        {
            objectToReturn.SetActive(false);
        }
    }
}

public interface IPooledObject
{
    void OnObjectSpawn();
}