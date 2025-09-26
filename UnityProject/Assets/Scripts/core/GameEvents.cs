using System;
using UnityEngine;

namespace Core.Events
{
    /// <summary>
    /// Zentrales Event System für die Kommunikation zwischen Komponenten
    /// </summary>
    public static class GameEvents 
    {
        // ==================== ENEMY EVENTS ====================
        /// <summary>Wird gefeuert wenn ein Enemy gespawnt wurde</summary>
        public static event Action<GameObject> OnEnemySpawned;
        
        /// <summary>Wird gefeuert wenn ein Enemy stirbt</summary>
        public static event Action<GameObject> OnEnemyDied;
        
        /// <summary>Wird gefeuert wenn ein Enemy Schaden nimmt</summary>
        public static event Action<GameObject, float> OnEnemyDamaged;
        
        // ==================== PLAYER EVENTS ====================
        /// <summary>Wird gefeuert wenn sich die Player Health ändert (current, max)</summary>
        public static event Action<int, int> OnPlayerHealthChanged;
        
        /// <summary>Wird gefeuert wenn der Player stirbt</summary>
        public static event Action OnPlayerDied;
        
        /// <summary>Wird gefeuert wenn der Player Schaden nimmt</summary>
        public static event Action<int> OnPlayerDamaged;
        
        // ==================== WAVE EVENTS ====================
        /// <summary>Wird gefeuert wenn eine neue Wave startet</summary>
        public static event Action<int> OnWaveStarted;
        
        /// <summary>Wird gefeuert wenn eine Wave abgeschlossen ist</summary>
        public static event Action<int> OnWaveCompleted;
        
        /// <summary>Wird gefeuert wenn sich die Anzahl verbleibender Enemies ändert</summary>
        public static event Action<int, int> OnEnemiesRemainingChanged; // current, total
        
        // ==================== TRIGGER METHODS ====================
        // Diese Methoden machen den Code sauberer und sicherer (null-check ist integriert)
        
        // Enemy Triggers
        public static void EnemySpawned(GameObject enemy) 
            => OnEnemySpawned?.Invoke(enemy);
        
        public static void EnemyDied(GameObject enemy) 
            => OnEnemyDied?.Invoke(enemy);
        
        public static void EnemyDamaged(GameObject enemy, float damage) 
            => OnEnemyDamaged?.Invoke(enemy, damage);
        
        // Player Triggers
        public static void PlayerHealthChanged(int current, int max) 
            => OnPlayerHealthChanged?.Invoke(current, max);
        
        public static void PlayerDied() 
            => OnPlayerDied?.Invoke();
        
        public static void PlayerDamaged(int damage) 
            => OnPlayerDamaged?.Invoke(damage);
        
        // Wave Triggers
        public static void WaveStarted(int waveNumber) 
            => OnWaveStarted?.Invoke(waveNumber);
        
        public static void WaveCompleted(int waveNumber) 
            => OnWaveCompleted?.Invoke(waveNumber);
        
        public static void EnemiesRemainingChanged(int current, int total) 
            => OnEnemiesRemainingChanged?.Invoke(current, total);
        
        // ==================== CLEANUP ====================
        /// <summary>
        /// Entfernt alle Event-Listener. Sollte beim Scene-Wechsel aufgerufen werden
        /// um Memory Leaks zu vermeiden.
        /// </summary>
        public static void ClearAllListeners()
        {
            // Enemy Events
            OnEnemySpawned = null;
            OnEnemyDied = null;
            OnEnemyDamaged = null;
            
            // Player Events
            OnPlayerHealthChanged = null;
            OnPlayerDied = null;
            OnPlayerDamaged = null;
            
            // Wave Events
            OnWaveStarted = null;
            OnWaveCompleted = null;
            OnEnemiesRemainingChanged = null;
        }
    }
}