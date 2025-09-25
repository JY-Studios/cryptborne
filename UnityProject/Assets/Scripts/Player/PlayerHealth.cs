using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 5;
    public int currentHealth;
    
    [Header("Damage Feedback")]
    public float invulnerabilityTime = 1f;
    private bool isInvulnerable = false;
    private Renderer playerRenderer;
    
    void Start()
    {
        currentHealth = maxHealth;
        playerRenderer = GetComponent<Renderer>();
    }
    
    public void TakeDamage(int damage)
    {
        if (isInvulnerable) return;
        
        currentHealth -= damage;
        Debug.Log($"Player hit! Health: {currentHealth}/{maxHealth}");
        
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(InvulnerabilityFlash());
        }
    }
    
    System.Collections.IEnumerator InvulnerabilityFlash()
    {
        isInvulnerable = true;
        
        // Blinken für Invulnerability
        for (int i = 0; i < 5; i++)
        {
            playerRenderer.enabled = false;
            yield return new WaitForSeconds(0.1f);
            playerRenderer.enabled = true;
            yield return new WaitForSeconds(0.1f);
        }
        
        isInvulnerable = false;
    }
    
    void Die()
    {
        Debug.Log("GAME OVER!");
        // Scene neu laden nach 1 Sekunde
        Invoke("ReloadScene", 1f);
    }
    
    void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}