using UnityEngine;
using Characters.Player;

namespace Characters.Enemies.States
{
    public class EnemyAttackState : EnemyBaseState
    {
        private bool hasDealtDamage = false;

        public override void Enter(EnemyStateManager enemy)
        {
            hasDealtDamage = false;
            
            // Animation: Attack - DIREKT!
            if (enemy.animator != null)
            {
                enemy.animator.CrossFade("1H_Melee_Attack_Chop", 0.1f);
                Debug.Log("Enemy: Playing 1H_Melee_Attack_Chop animation");
            }
            
            // Zum Spieler schauen
            if (enemy.player != null)
            {
                Vector3 directionToPlayer = (enemy.player.position - enemy.transform.position).normalized;
                directionToPlayer.y = 0;
                
                if (directionToPlayer.magnitude > 0.01f)
                {
                    enemy.transform.rotation = Quaternion.LookRotation(directionToPlayer);
                }
            }
        }

        public override void Update(EnemyStateManager enemy)
        {
            if (enemy.player == null || !enemy.health.IsAlive())
                return;

            float distanceToPlayer = Vector3.Distance(enemy.transform.position, enemy.player.position);

            // Damage bei ca. 50% der Animation austeilen
            if (!hasDealtDamage && Time.time >= enemy.nextAttackTime + 0.3f)
            {
                if (distanceToPlayer <= enemy.attackRange * 1.2f)
                {
                    PlayerHealth playerHealth = enemy.player.GetComponent<PlayerHealth>();
                    if (playerHealth != null)
                    {
                        playerHealth.TakeDamage(enemy.attackDamage);
                        Debug.Log($"Enemy dealt {enemy.attackDamage} damage to player!");
                    }
                }
                hasDealtDamage = true;
            }

            // Nach Cooldown nächsten State wählen
            if (Time.time >= enemy.nextAttackTime + enemy.attackCooldown)
            {
                enemy.nextAttackTime = Time.time;

                if (distanceToPlayer <= enemy.attackRange)
                {
                    // Nochmal angreifen
                    hasDealtDamage = false;
                    
                    if (enemy.animator != null)
                    {
                        enemy.animator.CrossFade("1H_Melee_Attack_Chop", 0.1f);
                    }
                }
                else if (distanceToPlayer <= enemy.detectionRange)
                {
                    enemy.SwitchState(enemy.chaseState);
                }
                else
                {
                    enemy.SwitchState(enemy.idleState);
                }
            }
        }

        public override void Exit(EnemyStateManager enemy)
        {
            hasDealtDamage = false;
        }
    }
}