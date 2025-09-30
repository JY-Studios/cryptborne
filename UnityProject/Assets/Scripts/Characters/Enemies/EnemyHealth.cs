using UnityEngine;
using Core.Events;

namespace Characters.Enemies
{
    public class EnemyHealth : MonoBehaviour
    {
        [Header("Health")] 
        public int maxHealth = 40; // Standard Enemy
        public int currentHealth;

        [Header("Visual Feedback")] 
        public Color damageColor = Color.yellow;
        private Color originalColor;
        private Renderer enemyRenderer;

        private EnemyStateManager stateManager;
        
        private const float DAMAGE_FLASH_DURATION = 0.1f;

        void Start()
        {
            Initialize();
        }

        void OnEnable()
        {
            Initialize();
        }

        void Initialize()
        {
            currentHealth = maxHealth;

            if (enemyRenderer == null)
                enemyRenderer = GetComponent<Renderer>();

            if (enemyRenderer != null)
            {
                if (originalColor == default(Color))
                    originalColor = enemyRenderer.material.color;
                else
                    enemyRenderer.material.color = originalColor;
            }

            if (stateManager == null)
                stateManager = GetComponent<EnemyStateManager>();
        }

        public void ResetHealth()
        {
            currentHealth = maxHealth;

            if (enemyRenderer != null)
            {
                enemyRenderer.material.color = originalColor;
                StopAllCoroutines();
            }
        }

        public void TakeDamage(int damage)
        {
            if (currentHealth <= 0) return;
            
            currentHealth -= damage;
            currentHealth = Mathf.Max(0, currentHealth);
            
            Debug.Log($"Enemy {gameObject.name} hit! Health: {currentHealth}/{maxHealth}");
            
            GameEvents.EnemyDamaged(gameObject, damage);

            if (enemyRenderer != null)
                StartCoroutine(DamageFlash());

            if (currentHealth <= 0)
                Die();
        }

        System.Collections.IEnumerator DamageFlash()
        {
            if (enemyRenderer != null)
            {
                enemyRenderer.material.color = damageColor;
                yield return new WaitForSeconds(DAMAGE_FLASH_DURATION);
                
                if (enemyRenderer != null)
                    enemyRenderer.material.color = originalColor;
            }
        }

        void Die()
        {
            Debug.Log($"Enemy {gameObject.name} died!");
            
            if (stateManager != null)
            {
                stateManager.SwitchState(stateManager.deadState);
            }
            else
            {
                GameEvents.EnemyDied(gameObject);
                ReturnToPool();
            }
        }

        void ReturnToPool()
        {
            PoolManager.Instance.Despawn("Enemy", gameObject);
        }
        
        public float GetHealthPercentage() => (float)currentHealth / maxHealth;
        public bool IsAlive() => currentHealth > 0;
        public bool IsDamaged() => currentHealth < maxHealth;
    }
}