using UnityEngine;

namespace Characters.Enemies.States
{
    public class EnemyDeadState : EnemyBaseState
    {
        public override void Enter(EnemyStateManager enemy)
        {
            Debug.Log("Enemy died (FSM).");
            enemy.StartCoroutine(ReturnToPoolAfterDelay(enemy.gameObject, 0.1f));
        }

        public override void Update(EnemyStateManager enemy) { }

        public override void Exit(EnemyStateManager enemy) { }
        
        System.Collections.IEnumerator ReturnToPoolAfterDelay(GameObject enemyObject, float delay)
        {
            yield return new WaitForSeconds(delay);
            PoolManager.Instance.Despawn("Enemy", enemyObject);
        }
    }
}