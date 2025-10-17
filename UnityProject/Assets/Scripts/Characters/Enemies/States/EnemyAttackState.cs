using UnityEngine;
using Characters.Player;

namespace Characters.Enemies.States
{
    public class EnemyAttackState : EnemyBaseState
    {
        private bool hasDealtDamage = false;
        private float attackTimer = 0f;
        private const float ATTACK_DURATION = 1f; // Gesamte Attack-Animation Dauer

        public override void Enter(EnemyStateManager enemy)
        {
            hasDealtDamage = false;
            attackTimer = 0f;
            
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
            
            // Setze nextAttackTime sofort
            enemy.nextAttackTime = Time.time;
        }

        public override void Update(EnemyStateManager enemy)
        {
            if (enemy.player == null || !enemy.health.IsAlive())
                return;

            attackTimer += Time.deltaTime;
            float distanceToPlayer = Vector3.Distance(enemy.transform.position, enemy.player.position);

            // Damage bei ca. 30-50% der Animation austeilen
            if (!hasDealtDamage && attackTimer >= 0.3f && attackTimer <= 0.6f)
            {
                // Größerer Range-Check für WebGL
                if (distanceToPlayer <= enemy.attackRange * 1.5f)
                {
                    PlayerHealth playerHealth = enemy.player.GetComponent<PlayerHealth>();
                    if (playerHealth != null)
                    {
                        playerHealth.TakeDamage(enemy.attackDamage);
                        Debug.Log($"Enemy dealt {enemy.attackDamage} damage to player! Distance: {distanceToPlayer}");
                        hasDealtDamage = true;
                    }
                }
            }

            // Nach kompletter Animation nächsten State wählen
            if (attackTimer >= ATTACK_DURATION)
            {
                enemy.nextAttackTime = Time.time;

                if (distanceToPlayer <= enemy.attackRange * 1.2f)
                {
                    // Nochmal angreifen
                    hasDealtDamage = false;
                    attackTimer = 0f;
                    
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
            attackTimer = 0f;
        }
    }
}