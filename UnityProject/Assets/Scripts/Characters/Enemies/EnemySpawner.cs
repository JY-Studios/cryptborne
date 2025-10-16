using System.Collections;
using System.Collections.Generic;
using Core.Events;
using TMPro;
using UnityEngine;

namespace Characters.Enemies
{
    public class EnemySpawner : MonoBehaviour
    {
        public static EnemySpawner Instance;

        [Header("Wave Settings")] public int startingEnemies = 5; // Enemies in Wave 1

        public int enemiesIncreasePerWave = 1; // Wie viele mehr pro Wave
        public float timeBetweenWaves = 5f; // Pause zwischen Waves
        public float spawnDelay = 0.3f; // Delay zwischen einzelnen Spawns

        [Header("Current Wave Info")] [SerializeField]
        private int currentWave;

        [SerializeField] private int enemiesInCurrentWave;
        [SerializeField] private int enemiesAlive;
        [SerializeField] private bool waveInProgress;
        [SerializeField] private bool isSpawning;

        [Header("UI References")] public TextMeshProUGUI waveText;

        public TextMeshProUGUI enemiesRemainingText;
        public GameObject waveCompletePanel;

        [Header("Auto-Detect Room")] public ModularRoomBuilder roomBuilder;

        public float wallOffset = 1.5f;

        public List<GameObject> ActiveEnemies { get; } = new();

        private void Start()
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

        private void OnDestroy()
        {
            // Events abmelden um Memory Leaks zu vermeiden
            GameEvents.OnEnemyDied -= OnEnemyDied;
        }

        private void OnEnemyDied(GameObject enemy)
        {
            if (waveInProgress && ActiveEnemies.Contains(enemy))
            {
                ActiveEnemies.Remove(enemy);
                enemiesAlive--;

                // Event für UI Updates
                GameEvents.EnemiesRemainingChanged(enemiesAlive, enemiesInCurrentWave);

                UpdateUI();

                // Wave nur beenden wenn ALLE gespawnt wurden UND alle tot sind
                if (enemiesAlive == 0 && !isSpawning) CompleteWave();
            }
        }

        private void CompleteWave()
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

            // WICHTIG: Upgrade Panel übernimmt die Kontrolle!
            if (UpgradeManager.Instance != null)
                UpgradeManager.Instance.ShowUpgradePanel();
            // UpgradeManager ruft dann StartNextWaveAfterDelay() auf
            else
                // Fallback falls kein UpgradeManager existiert
                StartCoroutine(StartNextWaveWithDelay(timeBetweenWaves));
        }

        private IEnumerator HideWaveCompletePanel()
        {
            yield return new WaitForSeconds(2f);
            if (waveCompletePanel != null)
                waveCompletePanel.SetActive(false);
        }

        private IEnumerator StartNextWaveWithDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            StartNextWave();
        }

        // Öffentliche Methode für UpgradeManager um Wave mit Delay zu starten
        public void StartNextWaveAfterDelay()
        {
            StartCoroutine(StartNextWaveWithDelay(timeBetweenWaves));
        }

        public void StartNextWave()
        {
            currentWave++;
            enemiesInCurrentWave = startingEnemies + enemiesIncreasePerWave * (currentWave - 1);
            enemiesAlive = enemiesInCurrentWave;
            waveInProgress = true;
            isSpawning = true;

            Debug.Log($"Starting Wave {currentWave} with {enemiesInCurrentWave} enemies");

            // Event auslösen
            GameEvents.WaveStarted(currentWave);

            // Enemies spawnen
            StartCoroutine(SpawnWaveEnemies());

            UpdateUI();
        }

        private IEnumerator SpawnWaveEnemies()
        {
            ActiveEnemies.Clear();
            var spawnedCount = 0;

            float roomWidth = roomBuilder ? roomBuilder.roomWidth : 10;
            float roomDepth = roomBuilder ? roomBuilder.roomDepth : 10;

            for (var i = 0; i < enemiesInCurrentWave; i++)
            {
                var randomPos = GetRandomSpawnPosition(roomWidth, roomDepth);

                // Spawn mit coolem Effekt
                var enemy = PoolManager.Instance.Spawn("Enemy", randomPos, Quaternion.identity);
                var esm = enemy.GetComponent<EnemyStateManager>();

                if (enemy != null)
                {
                    if (esm != null)
                        esm.ScaleStats(currentWave);
                    ActiveEnemies.Add(enemy);
                    spawnedCount++;

                    // Event auslösen für andere Systeme
                    GameEvents.EnemySpawned(enemy);

                    // Spawn Animation/Effekt
                    AddSpawnEffect(enemy);
                }
                else
                {
                    // Wenn Spawn fehlschlägt, enemiesAlive anpassen
                    enemiesAlive--;
                    Debug.LogWarning($"Could not spawn enemy {i + 1}/{enemiesInCurrentWave}");
                }

                // Kleine Pause zwischen Spawns für dramatischen Effekt
                yield return new WaitForSeconds(spawnDelay);
            }

            isSpawning = false;

            // Falls alle Enemies während des Spawnens getötet wurden
            if (enemiesAlive == 0) CompleteWave();

            Debug.Log($"Wave {currentWave}: Spawned {spawnedCount} enemies");
            GameEvents.EnemiesRemainingChanged(enemiesAlive, enemiesInCurrentWave);
        }

        private Vector3 GetRandomSpawnPosition(float roomWidth, float roomDepth)
        {
            var randomPos = Vector3.zero;
            var maxAttempts = 10;

            // Standard Y-Position als Fallback
            var fallbackY = 0.5f;

            for (var attempt = 0; attempt < maxAttempts; attempt++)
            {
                // 50% Chance für Spawn am Rand (dramatischer)
                if (Random.value > 0.5f)
                {
                    // Spawn am Rand
                    float edge = Random.Range(0, 4);
                    switch ((int)edge)
                    {
                        case 0: // Links
                            randomPos = new Vector3(wallOffset, fallbackY,
                                Random.Range(wallOffset, roomDepth - wallOffset));
                            break;
                        case 1: // Rechts
                            randomPos = new Vector3(roomWidth - wallOffset, fallbackY,
                                Random.Range(wallOffset, roomDepth - wallOffset));
                            break;
                        case 2: // Oben
                            randomPos = new Vector3(Random.Range(wallOffset, roomWidth - wallOffset), fallbackY,
                                roomDepth - wallOffset);
                            break;
                        default: // Unten
                            randomPos = new Vector3(Random.Range(wallOffset, roomWidth - wallOffset), fallbackY,
                                wallOffset);
                            break;
                    }
                }
                else
                {
                    // Zufällig im Raum
                    randomPos = new Vector3(
                        Random.Range(wallOffset, roomWidth - wallOffset),
                        fallbackY,
                        Random.Range(wallOffset, roomDepth - wallOffset)
                    );
                }

                // Versuche mit mehreren Raycast-Methoden die richtige Y-Position zu finden
                var finalPos = TryFindGroundPosition(randomPos);

                // Prüfe ob Position in Wand ist (mit SphereCast)
                if (!IsPositionInsideWall(finalPos)) return finalPos;
            }

            // Absoluter Fallback: Mitte des Raums
            Debug.LogWarning("Could not find valid spawn position after all attempts, using center");
            return new Vector3(roomWidth * 0.5f, fallbackY, roomDepth * 0.5f);
        }

        private Vector3 TryFindGroundPosition(Vector3 position)
        {
            // Mehrere Raycast-Versuche von verschiedenen Höhen
            var rayStart = position;
            rayStart.y = 10f; // Start hoch oben

            RaycastHit hit;

            // Versuch 1: Normaler Raycast nach unten
            if (Physics.Raycast(rayStart, Vector3.down, out hit, 15f))
                // Nur Ground akzeptieren, alles andere ignorieren
                if (hit.collider != null && hit.collider.CompareTag("Ground"))
                    return hit.point + Vector3.up * 0.1f;

            // Versuch 2: RaycastAll und ersten Ground finden
            var hits = Physics.RaycastAll(rayStart, Vector3.down, 15f);
            foreach (var h in hits)
                if (h.collider != null && h.collider.CompareTag("Ground"))
                    return h.point + Vector3.up * 0.1f;

            // Fallback: Original Position mit fester Y-Höhe
            position.y = 0.5f;
            return position;
        }

        private bool IsPositionInsideWall(Vector3 position)
        {
            // Prüfe mit kleinem Sphere ob Position frei ist
            var colliders = Physics.OverlapSphere(position, 0.3f);

            foreach (var col in colliders)
                if (col.CompareTag("Wall"))
                    return true; // Position ist in einer Wand

            return false; // Position ist frei
        }

        private void AddSpawnEffect(GameObject enemy)
        {
            // Scale Animation für Spawn
            enemy.transform.localScale = Vector3.zero;
            StartCoroutine(ScaleIn(enemy.transform));
        }

        private IEnumerator ScaleIn(Transform enemyTransform)
        {
            var duration = 0.3f;
            float elapsed = 0;

            while (elapsed < duration && enemyTransform != null)
            {
                elapsed += Time.deltaTime;
                var scale = Mathf.Lerp(0, 1, elapsed / duration);
                enemyTransform.localScale = Vector3.one * scale;
                yield return null;
            }

            if (enemyTransform != null)
                enemyTransform.localScale = Vector3.one;
        }

        private void UpdateUI()
        {
            if (waveText != null) waveText.text = $"Wave {currentWave}";

            if (enemiesRemainingText != null)
                enemiesRemainingText.text = $"Enemies: {enemiesAlive}/{enemiesInCurrentWave}";
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
            foreach (var enemy in ActiveEnemies)
                if (enemy != null)
                    PoolManager.Instance.Despawn("Enemy", enemy);

            ActiveEnemies.Clear();
            enemiesAlive = 0;
            isSpawning = false;
            GameEvents.EnemiesRemainingChanged(0, enemiesInCurrentWave);
        }

        // Wave Info für andere Systeme
        public int GetCurrentWave()
        {
            return currentWave;
        }

        public int GetEnemiesRemaining()
        {
            return enemiesAlive;
        }

        public bool IsWaveInProgress()
        {
            return waveInProgress;
        }

        // Öffentlicher Zugriff auf aktive Enemies (für andere Systeme)
        public List<GameObject> GetActiveEnemies()
        {
            return new List<GameObject>(ActiveEnemies);
        }
    }
}