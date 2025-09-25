using UnityEngine;

namespace Enemies.States
{
    public class EnemyAttackState : EnemyBaseState
    {
        public override void Enter(EnemyStateManager enemy) { }

        public override void Update(EnemyStateManager enemy)
        {
            if (enemy.player == null) return;

            float distance = Vector3.Distance(enemy.transform.position, enemy.player.position);

            if (distance > enemy.attackRange)
            {
                enemy.SwitchState(enemy.chaseState);
                return;
            }

            if (Time.time >= enemy.nextAttackTime)
            {
                PlayerHealth playerHealth = enemy.player.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(enemy.attackDamage);
                    Debug.Log("Enemy attacks player!");
                }
                enemy.nextAttackTime = Time.time + enemy.attackCooldown;
            }
        }

        public override void Exit(EnemyStateManager enemy) { }
    }
}