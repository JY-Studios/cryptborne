using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;
    public float detectionRange = 8f;
    public float attackRange = 1.5f;
    
    private Transform player;
    private bool isChasing = false;
    
    void Start()
    {
        // Finde den Player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }
    
    void Update()
    {
        if (player == null) return;
        
        float distance = Vector3.Distance(transform.position, player.position);
        
        // Spieler in Reichweite?
        if (distance <= detectionRange)
        {
            isChasing = true;
        }
        
        // Verfolgen
        if (isChasing && distance > attackRange)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            direction.y = 0; // Nur auf X/Z Ebene bewegen
            
            transform.position += direction * moveSpeed * Time.deltaTime;
            
            // Gegner zum Player drehen
            transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // Zeige Detection Range im Editor
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}