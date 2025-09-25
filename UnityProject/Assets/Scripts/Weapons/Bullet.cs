using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damage = 1;
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            // Health Component finden und Schaden machen
            EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
            }
            
            // Kugel zerstören
            Destroy(gameObject);
        }
        else if (!other.CompareTag("Player"))
        {
            // Bei Wand auch zerstören
            Destroy(gameObject);
        }
    }
}