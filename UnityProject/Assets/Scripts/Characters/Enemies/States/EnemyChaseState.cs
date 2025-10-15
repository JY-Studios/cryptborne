using UnityEngine;

namespace Characters.Enemies.States
{
    public class EnemyChaseState : EnemyBaseState
    {
        public override void Enter(EnemyStateManager enemy)
        {
            // Animation: Running - DIREKT!
            if (enemy.animator != null)
            {
                enemy.animator.CrossFade("Running_A", 0.1f);
                Debug.Log("Enemy: Playing Running_A animation");
            }
        }

        public override void Update(EnemyStateManager enemy)
        {
            if (enemy.player == null || !enemy.health.IsAlive())
                return;

            float distanceToPlayer = Vector3.Distance(enemy.transform.position, enemy.player.position);

            // Zu weit weg? Zurück zu Idle
            if (distanceToPlayer > enemy.detectionRange)
            {
                enemy.SwitchState(enemy.idleState);
                return;
            }

            // Nah genug zum Angreifen?
            if (distanceToPlayer <= enemy.attackRange)
            {
                enemy.SwitchState(enemy.attackState);
                return;
            }

            // Zum Spieler bewegen
            Vector3 directionToPlayer = (enemy.player.position - enemy.transform.position).normalized;
            
            // Separation von anderen Enemies
            Vector3 separationForce = CalculateSeparation(enemy);
            Vector3 finalDirection = (directionToPlayer + separationForce * 0.5f).normalized;

            // Bewegung ausführen
            Vector3 movement = finalDirection * enemy.moveSpeed * Time.deltaTime;
            
            if (enemy.controller != null && enemy.controller.enabled)
            {
                enemy.controller.Move(movement);
                
                // Zum Spieler rotieren
                if (directionToPlayer.magnitude > 0.01f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
                    enemy.transform.rotation = Quaternion.RotateTowards(
                        enemy.transform.rotation,
                        targetRotation,
                        360f * Time.deltaTime
                    );
                }
            }
        }

        public override void Exit(EnemyStateManager enemy)
        {
            // Nichts zu tun - Animation wird im nächsten State gesetzt
        }

        Vector3 CalculateSeparation(EnemyStateManager enemy)
        {
            Vector3 separationForce = Vector3.zero;
            int neighborCount = 0;

            if (EnemySpawner.Instance == null)
                return separationForce;

            var activeEnemies = EnemySpawner.Instance.ActiveEnemies;

            foreach (var otherEnemy in activeEnemies)
            {
                if (otherEnemy == null || otherEnemy == enemy.gameObject)
                    continue;

                float distance = Vector3.Distance(enemy.transform.position, otherEnemy.transform.position);

                if (distance < enemy.separationRadius && distance > 0.01f)
                {
                    Vector3 awayFromOther = (enemy.transform.position - otherEnemy.transform.position).normalized;
                    separationForce += awayFromOther / distance;
                    neighborCount++;
                }
            }

            if (neighborCount > 0)
                separationForce /= neighborCount;

            return separationForce;
        }
    }
}