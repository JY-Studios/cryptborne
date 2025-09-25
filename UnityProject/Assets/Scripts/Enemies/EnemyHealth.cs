using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 2;
    public int currentHealth;
    
    [Header("Visual Feedback")]
    public Color damageColor = Color.red;
    private Color originalColor;
    private Renderer enemyRenderer;
    
    void Start()
    {
        currentHealth = maxHealth;
        
        // Renderer für Farb-Feedback
        enemyRenderer = GetComponent<Renderer>();
        if (enemyRenderer != null)
            originalColor = enemyRenderer.material.color;
    }
    
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log($"Enemy hit! Health: {currentHealth}/{maxHealth}");
        
        // Visuelles Feedback
        if (enemyRenderer != null)
        {
            StartCoroutine(DamageFlash());
        }
        
        // Tot?
        if (currentHealth <= 0)
        {
            Die();
        }
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
        
        // Kleine Death-Animation (optional)
        transform.localScale = Vector3.one * 0.1f;
        
        // Enemy zerstören
        Destroy(gameObject, 0.1f);
    }
}