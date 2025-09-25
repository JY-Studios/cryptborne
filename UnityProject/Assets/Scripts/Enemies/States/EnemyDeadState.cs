using UnityEngine;

namespace Enemies.States
{
    public class EnemyDeadState : EnemyBaseState
    {
        public override void Enter(EnemyStateManager enemy)
        {
            Debug.Log("Enemy died (FSM).");
            GameObject.Destroy(enemy.gameObject, 0.1f);
        }

        public override void Update(EnemyStateManager enemy) { }

        public override void Exit(EnemyStateManager enemy) { }
    }
}