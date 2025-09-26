using Characters.Player;
using UnityEngine;
using TMPro;
using Core.Events;

public class HealthUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI enemiesText;
    
    [Header("Display Format")]
    public string healthFormat = "HP: {0}/{1}";
    public string waveFormat = "Wave {0}";
    public string enemiesFormat = "Enemies: {0}/{1}";
    
    // Cache für Performance
    private int lastHealth = -1;
    private int lastMaxHealth = -1;
    private int lastWave = -1;
    private int lastEnemiesRemaining = -1;
    private int lastTotalEnemies = -1;
    
    void OnEnable()
    {
        // Events abonnieren
        GameEvents.OnPlayerHealthChanged += OnPlayerHealthChanged;
        GameEvents.OnWaveStarted += OnWaveStarted;
        GameEvents.OnWaveCompleted += OnWaveCompleted;
        GameEvents.OnEnemiesRemainingChanged += OnEnemiesRemainingChanged;
        
        // Initial setup
        InitializeUI();
    }
    
    void OnDisable()
    {
        // Events abmelden (wichtig!)
        GameEvents.OnPlayerHealthChanged -= OnPlayerHealthChanged;
        GameEvents.OnWaveStarted -= OnWaveStarted;
        GameEvents.OnWaveCompleted -= OnWaveCompleted;
        GameEvents.OnEnemiesRemainingChanged -= OnEnemiesRemainingChanged;
    }
    
    void Start()
    {
        // Falls Text-Komponenten nicht zugewiesen sind, versuchen sie zu finden
        if (healthText == null)
            healthText = GameObject.Find("HealthText")?.GetComponent<TextMeshProUGUI>();
        
        if (waveText == null)
            waveText = GameObject.Find("WaveText")?.GetComponent<TextMeshProUGUI>();
        
        if (enemiesText == null)
            enemiesText = GameObject.Find("EnemiesText")?.GetComponent<TextMeshProUGUI>();
        
        InitializeUI();
    }
    
    void InitializeUI()
    {
        // Player Health initial setzen (falls vorhanden)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                UpdateHealthDisplay(playerHealth.currentHealth, playerHealth.maxHealth);
            }
        }
    }
    
    // ==================== EVENT HANDLERS ====================
    
    void OnPlayerHealthChanged(int current, int max)
    {
        UpdateHealthDisplay(current, max);
    }
    
    void OnWaveStarted(int waveNumber)
    {
        UpdateWaveDisplay(waveNumber);
    }
    
    void OnWaveCompleted(int waveNumber)
    {
        // Optional: Spezielle Anzeige für Wave Complete
        // Könnte z.B. kurz "Wave X Complete!" anzeigen
    }
    
    void OnEnemiesRemainingChanged(int current, int total)
    {
        UpdateEnemiesDisplay(current, total);
    }
    
    // ==================== UI UPDATE METHODS ====================
    
    void UpdateHealthDisplay(int current, int max)
    {
        // Nur updaten wenn sich was geändert hat (Performance!)
        if (current == lastHealth && max == lastMaxHealth)
            return;
        
        lastHealth = current;
        lastMaxHealth = max;
        
        if (healthText != null)
        {
            healthText.text = string.Format(healthFormat, current, max);
            
            // Optional: Farbe ändern basierend auf Health
            if (current <= max * 0.25f)
                healthText.color = Color.red;
            else if (current <= max * 0.5f)
                healthText.color = Color.yellow;
            else
                healthText.color = Color.white;
        }
    }
    
    void UpdateWaveDisplay(int waveNumber)
    {
        if (waveNumber == lastWave)
            return;
        
        lastWave = waveNumber;
        
        if (waveText != null)
        {
            waveText.text = string.Format(waveFormat, waveNumber);
        }
    }
    
    void UpdateEnemiesDisplay(int current, int total)
    {
        if (current == lastEnemiesRemaining && total == lastTotalEnemies)
            return;
        
        lastEnemiesRemaining = current;
        lastTotalEnemies = total;
        
        if (enemiesText != null)
        {
            enemiesText.text = string.Format(enemiesFormat, current, total);
            
            // Optional: Farbe ändern wenn fast alle tot
            if (current == 0)
                enemiesText.color = Color.green;
            else if (current <= 3)
                enemiesText.color = Color.yellow;
            else
                enemiesText.color = Color.white;
        }
    }
}