using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using Core.Events;

namespace Characters.Enemies
{
    public class EnemySpawner : MonoBehaviour
    {
        public static EnemySpawner Instance;
        
        [Header("Wave Settings")]
        public int startingEnemies = 5; // Enemies in Wave 1
        public int enemiesIncreasePerWave = 1; // Wie viele mehr pro Wave
        public float timeBetweenWaves = 5f; // Pause zwischen Waves
        public float spawnDelay = 0.3f; // Delay zwischen einzelnen Spawns
        
        [Header("Current Wave Info")]
        [SerializeField] private int currentWave = 0;
        [SerializeField] private int enemiesInCurrentWave = 0;
        [SerializeField] private int enemiesAlive = 0;
        [SerializeField] private bool waveInProgress = false;
        
        [Header("UI References")]
        public TextMeshProUGUI waveText;
        public TextMeshProUGUI enemiesRemainingText;
        public GameObject waveCompletePanel;
        
        [Header("Auto-Detect Room")]
        public ModularRoomBuilder roomBuilder;
        public float wallOffset = 1.5f;
        
        private List<GameObject> activeEnemies = new List<GameObject>();
        
        public List<GameObject> ActiveEnemies => activeEnemies;

        void Start()
        {
            Instance = this;
            
            if (roomBuilder == null)
                roomBuilder = FindObjectOfType<ModularRoomBuilder>();
            
            // UI initial verstecken
            if (waveCompletePanel != null)
                waveCompletePanel.SetActive(false);
            
            // Events abonnieren
            GameEvents.OnEnemyDied += OnEnemyDied;
            
            // Erste Wave starten
            StartCoroutine(StartNextWaveWithDelay(2f));
        }
        
        void OnDestroy()
        {
            // Events abmelden um Memory Leaks zu vermeiden
            GameEvents.OnEnemyDied -= OnEnemyDied;
        }
        
        void OnEnemyDied(GameObject enemy)
        {
            if (waveInProgress && activeEnemies.Contains(enemy))
            {
                activeEnemies.Remove(enemy);
                enemiesAlive = activeEnemies.Count;
                
                // Event für UI Updates
                GameEvents.EnemiesRemainingChanged(enemiesAlive, enemiesInCurrentWave);
                
                UpdateUI();
                
                // Wenn alle Enemies tot sind, Wave beenden
                if (enemiesAlive == 0)
                {
                    CompleteWave();
                }
            }
        }
        
        void CompleteWave()
        {
            waveInProgress = false;
            Debug.Log($"Wave {currentWave} complete!");
            
            // Event auslösen
            GameEvents.WaveCompleted(currentWave);
            
            // UI Update
            if (waveCompletePanel != null)
            {
                waveCompletePanel.SetActive(true);
                StartCoroutine(HideWaveCompletePanel());
            }
            
            // Nächste Wave starten
            StartCoroutine(StartNextWaveWithDelay(timeBetweenWaves));
        }
        
        IEnumerator HideWaveCompletePanel()
        {
            yield return new WaitForSeconds(2f);
            if (waveCompletePanel != null)
                waveCompletePanel.SetActive(false);
        }
        
        IEnumerator StartNextWaveWithDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            StartNextWave();
        }
        
        public void StartNextWave()
        {
            currentWave++;
            enemiesInCurrentWave = startingEnemies + (enemiesIncreasePerWave * (currentWave - 1));
            waveInProgress = true;
            
            Debug.Log($"Starting Wave {currentWave} with {enemiesInCurrentWave} enemies");
            
            // Event auslösen
            GameEvents.WaveStarted(currentWave);
            
            // Enemies spawnen
            StartCoroutine(SpawnWaveEnemies());
            
            UpdateUI();
        }
        
        IEnumerator SpawnWaveEnemies()
        {
            activeEnemies.Clear();
            
            float roomWidth = roomBuilder ? roomBuilder.roomWidth : 10;
            float roomDepth = roomBuilder ? roomBuilder.roomDepth : 10;
            
            for(int i = 0; i < enemiesInCurrentWave; i++)
            {
                Vector3 randomPos = GetRandomSpawnPosition(roomWidth, roomDepth);
                
                // Spawn mit coolem Effekt
                GameObject enemy = PoolManager.Instance.Spawn("Enemy", randomPos, Quaternion.identity);
                
                if (enemy != null)
                {
                    activeEnemies.Add(enemy);
                    
                    // Event auslösen für andere Systeme
                    GameEvents.EnemySpawned(enemy);
                    
                    // Spawn Animation/Effekt
                    AddSpawnEffect(enemy);
                }
                else
                {
                    Debug.LogWarning($"Could not spawn enemy {i+1}/{enemiesInCurrentWave}");
                }
                
                // Kleine Pause zwischen Spawns für dramatischen Effekt
                yield return new WaitForSeconds(spawnDelay);
            }
            
            enemiesAlive = activeEnemies.Count;
            GameEvents.EnemiesRemainingChanged(enemiesAlive, enemiesInCurrentWave);
            Debug.Log($"Wave {currentWave}: Spawned {enemiesAlive} enemies");
        }
        
        Vector3 GetRandomSpawnPosition(float roomWidth, float roomDepth)
        {
            // Zufällige Position mit mehr Variation
            Vector3 randomPos;
            
            // 50% Chance für Spawn am Rand (dramatischer)
            if (Random.value > 0.5f)
            {
                // Spawn am Rand
                float edge = Random.Range(0, 4);
                switch((int)edge)
                {
                    case 0: // Links
                        randomPos = new Vector3(wallOffset, 0.5f, Random.Range(wallOffset, roomDepth - wallOffset));
                        break;
                    case 1: // Rechts
                        randomPos = new Vector3(roomWidth - wallOffset, 0.5f, Random.Range(wallOffset, roomDepth - wallOffset));
                        break;
                    case 2: // Oben
                        randomPos = new Vector3(Random.Range(wallOffset, roomWidth - wallOffset), 0.5f, roomDepth - wallOffset);
                        break;
                    default: // Unten
                        randomPos = new Vector3(Random.Range(wallOffset, roomWidth - wallOffset), 0.5f, wallOffset);
                        break;
                }
            }
            else
            {
                // Zufällig im Raum
                randomPos = new Vector3(
                    Random.Range(wallOffset, roomWidth - wallOffset),
                    0.5f,
                    Random.Range(wallOffset, roomDepth - wallOffset)
                );
            }
            
            return randomPos;
        }
        
        void AddSpawnEffect(GameObject enemy)
        {
            // Scale Animation für Spawn
            enemy.transform.localScale = Vector3.zero;
            StartCoroutine(ScaleIn(enemy.transform));
            
            // Particle Effect spawnen (wenn vorhanden)
            // GameObject spawnVFX = PoolManager.Instance.Spawn("SpawnVFX", enemy.transform.position, Quaternion.identity);
        }
        
        IEnumerator ScaleIn(Transform enemyTransform)
        {
            float duration = 0.3f;
            float elapsed = 0;
            
            while (elapsed < duration && enemyTransform != null)
            {
                elapsed += Time.deltaTime;
                float scale = Mathf.Lerp(0, 1, elapsed / duration);
                enemyTransform.localScale = Vector3.one * scale;
                yield return null;
            }
            
            if (enemyTransform != null)
                enemyTransform.localScale = Vector3.one;
        }
        
        void UpdateUI()
        {
            if (waveText != null)
            {
                waveText.text = $"Wave {currentWave}";
            }
            
            if (enemiesRemainingText != null)
            {
                enemiesRemainingText.text = $"Enemies: {enemiesAlive}/{enemiesInCurrentWave}";
            }
        }
        
        // Manueller Wave Skip (für Testing)
        [ContextMenu("Skip to Next Wave")]
        public void SkipToNextWave()
        {
            ClearAllEnemies();
            CompleteWave();
        }
        
        // Alle Enemies entfernen
        public void ClearAllEnemies()
        {
            foreach(var enemy in activeEnemies)
            {
                if (enemy != null)
                    PoolManager.Instance.Despawn("Enemy", enemy);
            }
            activeEnemies.Clear();
            enemiesAlive = 0;
            GameEvents.EnemiesRemainingChanged(0, enemiesInCurrentWave);
        }
        
        // Wave Info für andere Systeme
        public int GetCurrentWave() => currentWave;
        public int GetEnemiesRemaining() => enemiesAlive;
        public bool IsWaveInProgress() => waveInProgress;
        
        // Öffentlicher Zugriff auf aktive Enemies (für andere Systeme)
        public List<GameObject> GetActiveEnemies() => new List<GameObject>(activeEnemies);
    }
}