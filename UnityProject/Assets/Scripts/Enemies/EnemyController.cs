using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;
    public float detectionRange = 8f;
    public float attackRange = 1.5f;
    public float separationRadius = 1f;
    
    private Transform player;
    private CharacterController controller;
    private bool isChasing = false;
    
    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
            
        // CharacterController statt Rigidbody
        controller = GetComponent<CharacterController>();
        if (controller == null)
            controller = gameObject.AddComponent<CharacterController>();
    }
    
    void Update()
    {
        if (player == null) return;
        
        float distance = Vector3.Distance(transform.position, player.position);
        
        if (distance <= detectionRange)
            isChasing = true;
        
        if (isChasing && distance > attackRange)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            direction.y = 0;
            
            // Separation von anderen Enemies
            Vector3 separation = GetSeparationVector();
            
            // Bewegung mit CharacterController
            Vector3 movement = (direction + separation) * moveSpeed * Time.deltaTime;
            controller.Move(movement);
            
            // Rotation
            if (direction != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(direction);
        }
    }
    
    Vector3 GetSeparationVector()
    {
        Vector3 separation = Vector3.zero;
        Collider[] neighbors = Physics.OverlapSphere(transform.position, separationRadius);
        
        foreach (Collider other in neighbors)
        {
            if (other.gameObject != gameObject && other.CompareTag("Enemy"))
            {
                Vector3 diff = transform.position - other.transform.position;
                diff.y = 0;
                if (diff.magnitude > 0)
                    separation += diff.normalized / diff.magnitude;
            }
        }
        
        return separation.normalized * 0.5f; // 0.5f = Separation Stärke
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}