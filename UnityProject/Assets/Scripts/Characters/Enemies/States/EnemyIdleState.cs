using UnityEngine;

namespace Characters.Enemies.States
{
    public class EnemyIdleState : EnemyBaseState
    {
        public override void Enter(EnemyStateManager enemy)
        {
            // Animation: Idle - DIREKT!
            if (enemy.animator != null)
            {
                enemy.animator.CrossFade("Idle", 0.1f);
                Debug.Log("Enemy: Playing Idle animation");
            }
            else
            {
                Debug.LogWarning("Enemy: No animator found for Idle!");
            }
        }

        public override void Update(EnemyStateManager enemy)
        {
            if (enemy.player == null || !enemy.health.IsAlive())
                return;

            // Spieler in Range? Chase starten
            float distanceToPlayer = Vector3.Distance(enemy.transform.position, enemy.player.position);

            if (distanceToPlayer <= enemy.detectionRange)
            {
                enemy.SwitchState(enemy.chaseState);
            }
        }

        public override void Exit(EnemyStateManager enemy)
        {
            // Nichts zu tun
        }
    }
}