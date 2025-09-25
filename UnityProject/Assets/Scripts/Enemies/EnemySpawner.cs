using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawning")]
    public GameObject enemyPrefab;
    public int enemyCount = 5;
    public float spawnRadius = 4f;
    
    void Start()
    {
        SpawnEnemies();
    }
    
    void SpawnEnemies()
    {
        for(int i = 0; i < enemyCount; i++)
        {
            // Zufällige Position im Raum
            Vector3 randomPos = new Vector3(
                Random.Range(-spawnRadius, spawnRadius),
                0.5f,  // Höhe für Enemy
                Random.Range(-spawnRadius, spawnRadius)
            );
            
            // Enemy spawnen
            Instantiate(enemyPrefab, randomPos, Quaternion.identity);
        }
    }
}