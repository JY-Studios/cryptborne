using Enemies;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 2;
    public int currentHealth;

    [Header("Visual Feedback")]
    public Color damageColor = Color.yellow;
    private Color originalColor;
    private Renderer enemyRenderer;

    private EnemyStateManager stateManager;

    void Start()
    {
        Initialize();
    }
    
    void OnEnable()
    {
        // Wird beim Pool-Recycling aufgerufen
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
        if (enemyRenderer != null)
            enemyRenderer.material.color = originalColor;
    }

    void Die()
    {
        Debug.Log("Enemy died!");
        if (stateManager != null)
            stateManager.SwitchState(stateManager.deadState);
        else
            ReturnToPool();
    }
    
    void ReturnToPool()
    {
        PoolManager.Instance.Despawn("Enemy", gameObject);
    }
}