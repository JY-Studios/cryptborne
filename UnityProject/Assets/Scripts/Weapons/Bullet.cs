using Characters.Enemies;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damage = 1;
    private bool hasHit = false;  // NEU: Verhindert Doppeltreffer
    
    void OnTriggerEnter(Collider other)
    {
        // Wenn schon getroffen, ignorieren
        if (hasHit) return;
        
        if (other.CompareTag("Enemy"))
        {
            hasHit = true;  // Sofort markieren!
            
            EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
            }
            
            Destroy(gameObject);
        }
        else if (!other.CompareTag("Player"))
        {
            hasHit = true;
            Destroy(gameObject);
        }
    }
}