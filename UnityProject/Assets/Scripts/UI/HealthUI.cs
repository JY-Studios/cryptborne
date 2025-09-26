using Characters.Player;
using UnityEngine;
using TMPro;

public class HealthUI : MonoBehaviour
{
    public TextMeshProUGUI healthText;
    private PlayerHealth playerHealth;
    
    void Start()
    {
        // Player finden
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
        }
        
        // Falls Text nicht zugewiesen, versuche es zu finden
        if (healthText == null)
        {
            healthText = GetComponent<TextMeshProUGUI>();
        }
    }
    
    void Update()
    {
        if (healthText != null && playerHealth != null)
        {
            healthText.text = $"HP: {playerHealth.currentHealth}/{playerHealth.maxHealth}";
        }
    }
}