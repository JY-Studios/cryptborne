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
        
        GameObject parent = new GameObject($"{poolName}_Pool");
        parent.transform.SetParent(transform);
        poolParents[poolName] = parent.transform;
        
        Queue<GameObject> objectPool = new Queue<GameObject>();
        activeObjects[poolName] = new List<GameObject>();
        prefabLookup[poolName] = prefab;
        
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
            GameObject prefab = prefabLookup[poolName];
            obj = Instantiate(prefab, poolParents[poolName]);
            obj.name = $"{poolName}_expanded_{activeObjects[poolName].Count}";
            Debug.LogWarning($"Pool '{poolName}' expanded!");
        }
    
        obj.transform.SetParent(null);
    
        CharacterController cc = obj.GetComponent<CharacterController>();
        if (cc != null)
            cc.enabled = false;
    
        obj.transform.position = position;
        obj.transform.rotation = rotation;
    
        ResetPooledObject(obj, poolName);
    
        if (cc != null)
            cc.enabled = true;
    
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
    
    public void DespawnAuto(GameObject obj)
    {
        if (obj == null) return;
        
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
            case "Enemy":
                EnemyHealth health = obj.GetComponent<EnemyHealth>();
                if (health != null) health.ResetHealth();
            
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
    
    // CLEANUP beim Beenden der Application/Scene
    void OnApplicationQuit()
    {
        CleanupPools();
    }
    
    void OnDestroy()
    {
        // Nur cleanup wenn dies die Instance ist
        if (instance == this)
        {
            CleanupPools();
            instance = null;
        }
    }
    
    void CleanupPools()
    {
        if (poolDictionary == null) return;
        
        // Despawn alle aktiven Objekte
        DespawnAllPools();
        
        // Destroy alle Pool-Parents und ihre Children
        foreach (var parent in poolParents.Values)
        {
            if (parent != null)
            {
                // Destroy alle Children
                foreach (Transform child in parent)
                {
                    if (child != null)
                        Destroy(child.gameObject);
                }
                
                // Destroy Parent
                Destroy(parent.gameObject);
            }
        }
        
        // Clear alle Dictionaries
        poolDictionary.Clear();
        poolParents.Clear();
        activeObjects.Clear();
        prefabLookup.Clear();
        
        Debug.Log("PoolManager cleaned up successfully");
    }
}