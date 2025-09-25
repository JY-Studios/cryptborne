using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour
{
    [Header("Settings")]
    public int damage = 1;
    public float lifetime = 2f;
    
    private bool hasHit = false;
    private Coroutine lifetimeCoroutine;
    
    void OnEnable()
    {
        lifetimeCoroutine = StartCoroutine(LifetimeTimer());
    }
    
    void OnDisable()
    {
        if (lifetimeCoroutine != null)
        {
            StopCoroutine(lifetimeCoroutine);
            lifetimeCoroutine = null;
        }
    }
    
    void OnTriggerEnter(Collider other)
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
        
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}