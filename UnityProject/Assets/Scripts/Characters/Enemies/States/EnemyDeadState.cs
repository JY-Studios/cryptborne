using UnityEngine;
using Core.Events;

namespace Characters.Enemies.States
{
    public class EnemyDeadState : EnemyBaseState
    {
        private float deathTime;
        private const float DEATH_DURATION = 2f;

        public override void Enter(EnemyStateManager enemy)
        {
            deathTime = Time.time;

            // Animation: Death - DIREKT!
            if (enemy.animator != null)
            {
                enemy.animator.CrossFade("Death_A", 0.1f);
                Debug.Log("Enemy: Playing Death_A animation");
            }

            // CharacterController deaktivieren
            if (enemy.controller != null)
            {
                enemy.controller.enabled = false;
            }

            // Event für andere Systeme
            GameEvents.EnemyDied(enemy.gameObject);

            Debug.Log($"Enemy {enemy.gameObject.name} entered death state");
        }

        public override void Update(EnemyStateManager enemy)
        {
            // Nach Death-Animation zurück in Pool
            if (Time.time >= deathTime + DEATH_DURATION)
            {
                ReturnToPool(enemy);
            }
        }

        public override void Exit(EnemyStateManager enemy)
        {
            // CharacterController wieder aktivieren
            if (enemy.controller != null)
            {
                enemy.controller.enabled = true;
            }
        }

        private void ReturnToPool(EnemyStateManager enemy)
        {
            if (PoolManager.Instance != null)
            {
                PoolManager.Instance.Despawn("Enemy", enemy.gameObject);
            }
            else
            {
                Object.Destroy(enemy.gameObject);
            }
        }
    }
}