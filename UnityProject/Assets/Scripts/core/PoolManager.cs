using System.Collections.Generic;
using Characters.Enemies;
using UnityEngine;

[System.Serializable]
public class PoolInfo
{
    public string poolName;
    public GameObject prefab;
    public int poolSize;
}

public class PoolManager : MonoBehaviour
{
    private static PoolManager instance;
    public static PoolManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<PoolManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("PoolManager");
                    instance = go.AddComponent<PoolManager>();
                }
            }
            return instance;
        }
    }
    
    [Header("Pool Configuration")]
    public List<PoolInfo> pools = new List<PoolInfo>()
    {
        new PoolInfo { poolName = "Bullet", prefab = null, poolSize = 30 },
        new PoolInfo { poolName = "Enemy", prefab = null, poolSize = 20 },
        new PoolInfo { poolName = "Particle", prefab = null, poolSize = 15 }
    };
    
    private Dictionary<string, Queue<GameObject>> poolDictionary;
    private Dictionary<string, Transform> poolParents;
    private Dictionary<string, List<GameObject>> activeObjects;
    private Dictionary<string, GameObject> prefabLookup;
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        InitializePools();
    }
    
    void InitializePools()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();
        poolParents = new Dictionary<string, Transform>();
        activeObjects = new Dictionary<string, List<GameObject>>();
        prefabLookup = new Dictionary<string, GameObject>();
        
        foreach (PoolInfo pool in pools)
        {
            if (pool.prefab == null)
            {
                Debug.LogWarning($"Pool '{pool.poolName}' has no prefab assigned!");
                continue;
            }
            
            CreatePool(pool.poolName, pool.prefab, pool.poolSize);
        }
    }
    
    public void CreatePool(string poolName, GameObject prefab, int size)
    {
        if (prefab == null) return;
        
        // Parent für Organisation
        GameObject parent = new GameObject($"{poolName}_Pool");
        parent.transform.SetParent(transform);
        poolParents[poolName] = parent.transform;
        
        // Queue für inaktive Objekte
        Queue<GameObject> objectPool = new Queue<GameObject>();
        
        // Liste für aktive Objekte
        activeObjects[poolName] = new List<GameObject>();
        
        // Prefab speichern für späteren Zugriff
        prefabLookup[poolName] = prefab;
        
        // Pool füllen
        for (int i = 0; i < size; i++)
        {
            GameObject obj = Instantiate(prefab, parent.transform);
            obj.name = $"{poolName}_{i}";
            obj.SetActive(false);
            objectPool.Enqueue(obj);
        }
        
        poolDictionary[poolName] = objectPool;
        Debug.Log($"Created pool '{poolName}' with {size} objects");
    }
    
    public GameObject Spawn(string poolName, Vector3 position, Quaternion rotation, GameObject fallbackPrefab = null, int defaultSize = 10)
    {
        // Wenn der Pool nicht existiert → neu erstellen
        if (!poolDictionary.ContainsKey(poolName))
        {
            if (fallbackPrefab == null)
            {
                Debug.LogError($"Pool '{poolName}' doesn't exist and no prefab was provided!");
                return null;
            }

            CreatePool(poolName, fallbackPrefab, defaultSize);
        }
    
        GameObject obj = null;
        Queue<GameObject> pool = poolDictionary[poolName];
    
        if (pool.Count > 0)
        {
            obj = pool.Dequeue();
        }
        else
        {
            // Pool ist leer - expandiere automatisch
            GameObject prefab = prefabLookup[poolName];
            obj = Instantiate(prefab, poolParents[poolName]);
            obj.name = $"{poolName}_expanded_{activeObjects[poolName].Count}";
            Debug.LogWarning($"Pool '{poolName}' expanded!");
        }
    
        // Aus Pool-Parent nehmen
        obj.transform.SetParent(null);
    
        // CharacterController temporär disablen (wichtig für Enemies!)
        CharacterController cc = obj.GetComponent<CharacterController>();
        if (cc != null)
            cc.enabled = false;
    
        // Position und Rotation setzen
        obj.transform.position = position;
        obj.transform.rotation = rotation;
    
        // Component-Reset OHNE Position zu ändern
        ResetPooledObject(obj, poolName);
    
        // CharacterController wieder enablen
        if (cc != null)
            cc.enabled = true;
    
        // GameObject aktivieren
        obj.SetActive(true);
        activeObjects[poolName].Add(obj);
    
        return obj;
    }
    
    public void Despawn(string poolName, GameObject obj)
    {
        if (obj == null) return;
        
        if (!poolDictionary.ContainsKey(poolName))
        {
            Debug.LogError($"Pool '{poolName}' doesn't exist!");
            return;
        }
        
        obj.SetActive(false);
        obj.transform.SetParent(poolParents[poolName]);
        
        activeObjects[poolName].Remove(obj);
        poolDictionary[poolName].Enqueue(obj);
    }
    
    // Automatische Pool-Erkennung basierend auf GameObject
    public void DespawnAuto(GameObject obj)
    {
        // Finde den richtigen Pool basierend auf dem Namen
        foreach (var poolName in poolDictionary.Keys)
        {
            if (activeObjects[poolName].Contains(obj))
            {
                Despawn(poolName, obj);
                return;
            }
        }
        
        Debug.LogWarning($"Object {obj.name} not found in any pool!");
    }
    
    void ResetPooledObject(GameObject obj, string poolName)
    {
        switch (poolName)
        {
            // case "Bullet":
            //     Bullet bullet = obj.GetComponent<Bullet>();
            //     if (bullet != null) bullet.ResetBullet();
            //     break;
            
            case "Enemy":
                // Enemy Health reset
                EnemyHealth health = obj.GetComponent<EnemyHealth>();
                if (health != null) health.ResetHealth();
            
                // Enemy State reset - OHNE Position zu ändern!
                EnemyStateManager stateManager = obj.GetComponent<EnemyStateManager>();
                if (stateManager != null) stateManager.ResetEnemy();
                break;
            
            case "Particle":
                ParticleSystem ps = obj.GetComponent<ParticleSystem>();
                if (ps != null) ps.Clear();
                break;
        }
    }
    
    public void DespawnAll(string poolName)
    {
        if (!activeObjects.ContainsKey(poolName)) return;
        
        GameObject[] active = activeObjects[poolName].ToArray();
        foreach (GameObject obj in active)
        {
            Despawn(poolName, obj);
        }
    }
    
    public void DespawnAllPools()
    {
        foreach (string poolName in poolDictionary.Keys)
        {
            DespawnAll(poolName);
        }
    }
    
    public int GetActiveCount(string poolName)
    {
        return activeObjects.ContainsKey(poolName) ? activeObjects[poolName].Count : 0;
    }
    
    public int GetPoolSize(string poolName)
    {
        return poolDictionary.ContainsKey(poolName) ? poolDictionary[poolName].Count : 0;
    }
}