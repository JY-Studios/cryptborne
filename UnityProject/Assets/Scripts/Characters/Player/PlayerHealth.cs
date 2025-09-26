using UnityEngine;
using UnityEngine.SceneManagement;
using Core.Events;

namespace Characters.Player
{
    public class PlayerHealth : MonoBehaviour
    {
        [Header("Health")]
        public int maxHealth = 5;
        public int currentHealth;
    
        [Header("Damage Feedback")]
        public float invulnerabilityTime = 1f;
        private bool isInvulnerable = false;
        private Renderer playerRenderer;
        
        // Constants
        private const float INVULNERABILITY_FLASH_DURATION = 0.1f;
        private const int INVULNERABILITY_FLASH_COUNT = 5;
        private const float DEATH_RELOAD_DELAY = 1f;
    
        void Start()
        {
            currentHealth = maxHealth;
            playerRenderer = GetComponent<Renderer>();
            
            // Initial health event
            GameEvents.PlayerHealthChanged(currentHealth, maxHealth);
        }
    
        public void TakeDamage(int damage)
        {
            if (isInvulnerable) return;
        
            currentHealth -= damage;
            currentHealth = Mathf.Max(0, currentHealth); // Nie unter 0
            
            Debug.Log($"Player hit! Health: {currentHealth}/{maxHealth}");
            
            // Events auslösen
            GameEvents.PlayerDamaged(damage);
            GameEvents.PlayerHealthChanged(currentHealth, maxHealth);
        
            if (currentHealth <= 0)
            {
                Die();
            }
            else
            {
                StartCoroutine(InvulnerabilityFlash());
            }
        }
        
        public void Heal(int amount)
        {
            int oldHealth = currentHealth;
            currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
            
            if (currentHealth != oldHealth)
            {
                Debug.Log($"Player healed! Health: {currentHealth}/{maxHealth}");
                GameEvents.PlayerHealthChanged(currentHealth, maxHealth);
            }
        }
    
        System.Collections.IEnumerator InvulnerabilityFlash()
        {
            isInvulnerable = true;
        
            // Blinken für Invulnerability
            if (playerRenderer != null)
            {
                for (int i = 0; i < INVULNERABILITY_FLASH_COUNT; i++)
                {
                    playerRenderer.enabled = false;
                    yield return new WaitForSeconds(INVULNERABILITY_FLASH_DURATION);
                    playerRenderer.enabled = true;
                    yield return new WaitForSeconds(INVULNERABILITY_FLASH_DURATION);
                }
            }
            else
            {
                // Falls kein Renderer, warte trotzdem die Zeit ab
                yield return new WaitForSeconds(invulnerabilityTime);
            }
        
            isInvulnerable = false;
        }
    
        void Die()
        {
            Debug.Log("GAME OVER!");
            
            // Event auslösen
            GameEvents.PlayerDied();
            
            // Disable player controls
            PlayerController controller = GetComponent<PlayerController>();
            if (controller != null)
                controller.enabled = false;
            
            // Scene neu laden nach Verzögerung
            Invoke(nameof(ReloadScene), DEATH_RELOAD_DELAY);
        }
    
        void ReloadScene()
        {
            // Events clearen bevor Scene reload
            GameEvents.ClearAllListeners();
            
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        
        // Public Getters
        public float GetHealthPercentage() => (float)currentHealth / maxHealth;
        public bool IsAlive() => currentHealth > 0;
        public bool IsInvulnerable() => isInvulnerable;
    }
}