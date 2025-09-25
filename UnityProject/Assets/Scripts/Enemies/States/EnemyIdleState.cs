using UnityEngine;

namespace Enemies.States
{
    public class EnemyIdleState : EnemyBaseState
    {
        public override void Enter(EnemyStateManager enemy)
        {
            // z.B. Idle-Animation starten
        }

        public override void Update(EnemyStateManager enemy)
        {
            if (enemy.player == null) return;

            float distance = Vector3.Distance(enemy.transform.position, enemy.player.position);

            if (distance <= enemy.detectionRange)
                enemy.SwitchState(enemy.chaseState);
        }

        public override void Exit(EnemyStateManager enemy) { }
    }
}