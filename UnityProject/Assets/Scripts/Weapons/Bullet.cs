using Characters.Enemies;
using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour
{
    [Header("Settings")]
    public int damage = 1;
    public float lifetime = 2f;
    
    [Header("Collision Fix")]
    public bool useRaycast = true;  // Aktiviere Raycast für perfekte Kollision
    private Vector3 lastPosition;
    
    private bool hasHit = false;
    private Coroutine lifetimeCoroutine;
    private Rigidbody rb;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        
        // WICHTIG: Continuous Collision Detection für schnelle Objekte
        if (rb != null)
        {
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }
    }
    
    void OnEnable()
    {
        lifetimeCoroutine = StartCoroutine(LifetimeTimer());
        lastPosition = transform.position;
    }
    
    void OnDisable()
    {
        if (lifetimeCoroutine != null)
        {
            StopCoroutine(lifetimeCoroutine);
            lifetimeCoroutine = null;
        }
    }
    
    void FixedUpdate()
    {
        // Raycast zwischen letzter und aktueller Position
        // Damit fangen wir auch sehr schnelle Kollisionen ab!
        if (useRaycast && !hasHit)
        {
            RaycastCheck();
        }
        
        lastPosition = transform.position;
    }
    
    void RaycastCheck()
    {
        Vector3 direction = transform.position - lastPosition;
        float distance = direction.magnitude;
        
        if (distance > 0.01f)  // Nur wenn sich die Bullet bewegt hat
        {
            RaycastHit hit;
            // Raycast von letzter zu aktueller Position
            if (Physics.Raycast(lastPosition, direction.normalized, out hit, distance))
            {
                // Prüfe ob wir etwas Relevantes getroffen haben
                ProcessHit(hit.collider);
            }
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        ProcessHit(other);
    }
    
    void ProcessHit(Collider other)
    {
        if (hasHit) return;
        
        // Player ignorieren
        if (other.CompareTag("Player"))
            return;
        
        // Bei Enemy: Schaden zufügen
        if (other.CompareTag("Enemy"))
        {
            hasHit = true;
            
            EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
                Debug.Log($"Enemy hit by bullet! (Method: {(useRaycast ? "Raycast" : "Trigger")})");
            }
            
            ReturnToPool();
        }
        // Bei Wall oder Ground: Zerstören
        else if (other.CompareTag("Wall") || other.CompareTag("Ground"))
        {
            hasHit = true;
            ReturnToPool();
        }
    }
    
    IEnumerator LifetimeTimer()
    {
        yield return new WaitForSeconds(lifetime);
        ReturnToPool();
    }
    
    void ReturnToPool()
    {
        PoolManager.Instance.Despawn("Bullet", gameObject);
    }
    
    public void ResetBullet()
    {
        hasHit = false;
        lastPosition = transform.position;
        
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            // Stelle sicher, dass Continuous Detection aktiv ist
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }
    }
}