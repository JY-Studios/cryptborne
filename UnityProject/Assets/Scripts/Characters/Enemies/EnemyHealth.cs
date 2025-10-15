using UnityEngine;
using Core.Events;
using System.Collections.Generic;

namespace Characters.Enemies
{
    public class EnemyHealth : MonoBehaviour
    {
        [Header("Health")] 
        public int maxHealth = 40;
        public int currentHealth;

        [Header("Visual Feedback")] 
        public Color damageColor = Color.yellow;
        private List<Material> originalMaterials = new List<Material>();
        private List<Renderer> enemyRenderers = new List<Renderer>();

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

            if (enemyRenderers.Count == 0)
            {
                // Alle Renderer im Character finden (inkl. Children)
                enemyRenderers.AddRange(GetComponentsInChildren<Renderer>());
                
                // Original Materials speichern
                originalMaterials.Clear();
                foreach (var renderer in enemyRenderers)
                {
                    if (renderer != null && renderer.material != null)
                    {
                        // Kopie des Materials erstellen (wichtig für Instancing!)
                        originalMaterials.Add(new Material(renderer.material));
                    }
                }
            }
            else
            {
                // Bei Re-Enable: Original-Farben wiederherstellen
                ResetColors();
            }

            if (stateManager == null)
                stateManager = GetComponent<EnemyStateManager>();
        }

        void ResetColors()
        {
            for (int i = 0; i < enemyRenderers.Count; i++)
            {
                if (enemyRenderers[i] != null && i < originalMaterials.Count)
                {
                    enemyRenderers[i].material.color = originalMaterials[i].color;
                }
            }
        }

        public void ResetHealth()
        {
            currentHealth = maxHealth;
            ResetColors();
            StopAllCoroutines();
        }

        public void TakeDamage(int damage)
        {
            if (currentHealth <= 0) return;
            
            currentHealth -= damage;
            currentHealth = Mathf.Max(0, currentHealth);
            
            Debug.Log($"Enemy {gameObject.name} hit! Health: {currentHealth}/{maxHealth}");
            
            GameEvents.EnemyDamaged(gameObject, damage);

            if (enemyRenderers.Count > 0)
                StartCoroutine(DamageFlash());

            if (currentHealth <= 0)
                Die();
        }

        System.Collections.IEnumerator DamageFlash()
        {
            // Alle Meshes auf Damage-Color setzen
            foreach (var renderer in enemyRenderers)
            {
                if (renderer != null)
                    renderer.material.color = damageColor;
            }
            
            yield return new WaitForSeconds(DAMAGE_FLASH_DURATION);
            
            // Zurück zur Original-Farbe
            ResetColors();
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