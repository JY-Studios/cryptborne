using UnityEngine;

namespace Characters.Enemies
{
    public class EnemySpawner : MonoBehaviour
    {
        [Header("Spawning")]
        public GameObject enemyPrefab;
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
            
                Instantiate(enemyPrefab, randomPos, Quaternion.identity);
            }
        }
    }
}