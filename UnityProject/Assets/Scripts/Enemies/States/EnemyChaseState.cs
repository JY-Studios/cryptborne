using UnityEngine;

namespace Enemies.States
{
    public class EnemyChaseState : EnemyBaseState
    {
        public override void Enter(EnemyStateManager enemy) { }

        public override void Update(EnemyStateManager enemy)
        {
            if (enemy.player == null) return;

            float distance = Vector3.Distance(enemy.transform.position, enemy.player.position);

            if (distance <= enemy.attackRange)
            {
                enemy.SwitchState(enemy.attackState);
                return;
            }

            // Bewegung + Separation
            Vector3 direction = (enemy.player.position - enemy.transform.position).normalized;
            direction.y = 0;

            Vector3 separation = GetSeparationVector(enemy);

            Vector3 movement = (direction + separation) * enemy.moveSpeed * Time.deltaTime;
            enemy.controller.Move(movement);

            if (direction != Vector3.zero)
                enemy.transform.rotation = Quaternion.LookRotation(direction);
        }

        public override void Exit(EnemyStateManager enemy) { }

        private Vector3 GetSeparationVector(EnemyStateManager enemy)
        {
            Vector3 separation = Vector3.zero;
            Collider[] neighbors = Physics.OverlapSphere(enemy.transform.position, enemy.separationRadius);

            foreach (Collider other in neighbors)
            {
                if (other.gameObject != enemy.gameObject && other.CompareTag("Enemy"))
                {
                    Vector3 diff = enemy.transform.position - other.transform.position;
                    diff.y = 0;
                    if (diff.magnitude > 0)
                        separation += diff.normalized / diff.magnitude;
                }
            }

            return separation.normalized * 0.5f;
        }
    }
}