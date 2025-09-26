using UnityEngine;
using Core.Events;

namespace Characters.Enemies.States
{
    public class EnemyDeadState : EnemyBaseState
    {
        public override void Enter(EnemyStateManager enemy)
        {
            Debug.Log($"Enemy {enemy.gameObject.name} died!");
            
            // Event auslösen BEVOR das GameObject zurück in den Pool geht
            GameEvents.EnemyDied(enemy.gameObject);
            
            // Kurze Verzögerung für Death-Effekte
            enemy.StartCoroutine(ReturnToPoolAfterDelay(enemy.gameObject, 0.1f));
        }

        public override void Update(EnemyStateManager enemy) 
        { 
            // Dead enemies don't update
        }

        public override void Exit(EnemyStateManager enemy) 
        { 
            // Cleanup wenn nötig
        }
        
        System.Collections.IEnumerator ReturnToPoolAfterDelay(GameObject enemyObject, float delay)
        {
            yield return new WaitForSeconds(delay);
            
            // Optional: Death particle effect spawnen
            // GameObject deathVFX = PoolManager.Instance.Spawn("DeathVFX", enemyObject.transform.position, Quaternion.identity);
            
            // Zurück in den Pool
            PoolManager.Instance.Despawn("Enemy", enemyObject);
        }
    }
}