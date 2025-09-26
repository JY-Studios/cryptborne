using UnityEngine;

namespace Characters.Enemies
{
    using UnityEngine;

    public class EnemySpawner : MonoBehaviour
    {
        [Header("Spawning")]
        public int enemyCount = 5;
    
        [Header("Auto-Detect Room")]
        public ModularRoomBuilder roomBuilder;
        public float wallOffset = 1.5f; // Abstand zu Wänden
    
        void Start()
        {
            if (roomBuilder == null)
                roomBuilder = FindObjectOfType<ModularRoomBuilder>();
            
            SpawnEnemies();
        }
    
        void SpawnEnemies()
        {
            float roomWidth = roomBuilder ? roomBuilder.roomWidth : 10;
            float roomDepth = roomBuilder ? roomBuilder.roomDepth : 10;
        
            for(int i = 0; i < enemyCount; i++)
            {
                Vector3 randomPos = new Vector3(
                    Random.Range(wallOffset, roomWidth - wallOffset),
                    0.5f,
                    Random.Range(wallOffset, roomDepth - wallOffset)
                );
            
                GameObject enemy = PoolManager.Instance.Spawn("Enemy", randomPos, Quaternion.identity);
            
                if (enemy == null)
                {
                    Debug.LogWarning($"Could not spawn enemy {i+1}/{enemyCount}");
                }
            }
        
            Debug.Log($"Spawned {enemyCount} enemies");
        }
    
        // Alle Enemies entfernen
        public void ClearAllEnemies()
        {
            PoolManager.Instance.DespawnAll("Enemy");
        }
    }
}