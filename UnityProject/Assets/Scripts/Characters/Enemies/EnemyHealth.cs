using UnityEngine;

namespace Characters.Enemies
{
    public class EnemyHealth : MonoBehaviour
    {
        [Header("Health")]
        public float maxHealth = 2;
        public float currentHealth;

        [Header("Visual Feedback")]
        public Color damageColor = Color.yellow;
        private Color originalColor;
        private Renderer enemyRenderer;

        private EnemyStateManager stateManager;

        void Start()
        {
            currentHealth = maxHealth;

            enemyRenderer = GetComponent<Renderer>();
            if (enemyRenderer != null)
                originalColor = enemyRenderer.material.color;

            stateManager = GetComponent<EnemyStateManager>();
        }

        public void TakeDamage(float damage)
        {
            currentHealth -= damage;
            Debug.Log($"Enemy hit! Health: {currentHealth}/{maxHealth}");

            if (enemyRenderer != null)
                StartCoroutine(DamageFlash());

            if (currentHealth <= 0)
                Die();
        }

        System.Collections.IEnumerator DamageFlash()
        {
            enemyRenderer.material.color = damageColor;
            yield return new WaitForSeconds(0.1f);
            enemyRenderer.material.color = originalColor;
        }

        void Die()
        {
            Debug.Log("Enemy died!");

            if (stateManager != null)
                stateManager.SwitchState(stateManager.deadState);
            else
                Destroy(gameObject, 0.1f);
        }
    }
}