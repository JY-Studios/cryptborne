using UnityEngine;
using System.Linq;

namespace Characters.Player
{
    public class PlayerShoot : MonoBehaviour
    {
        [Header("Shooting")]
        public Transform firePoint;
        public float bulletSpeed = 20f;
        public float shootInterval = 2f; // Schießintervall in Sekunden
        
        [Header("Targeting")]
        public float detectionRange = 8f; // Maximale Reichweite für Feinderkennung
        
        private float nextShootTime = 0f;

        void Update()
        {
            // Automatisches Schießen alle 3 Sekunden
            if (Time.time >= nextShootTime)
            {
                FindAndShootNearestEnemy();
                nextShootTime = Time.time + shootInterval;
            }
        }

        void FindAndShootNearestEnemy()
        {
            // Finde den nächsten Feind
            GameObject nearestEnemy = FindNearestEnemy();
            
            if (nearestEnemy != null)
            {
                // Richte den Charakter zum Feind aus
                Vector3 directionToEnemy = (nearestEnemy.transform.position - transform.position).normalized;
                directionToEnemy.y = 0; // Nur horizontale Rotation
                
                // Optional: Charakter zum Feind drehen
                transform.rotation = Quaternion.LookRotation(directionToEnemy);
                
                // Schieße in Richtung des Feindes
                Shoot(directionToEnemy);
            }
            else
            {
                Debug.Log("Kein Feind in Reichweite gefunden");
            }
        }

        GameObject FindNearestEnemy()
        {
            GameObject nearestEnemy = null;
            float nearestDistance = float.MaxValue;
            
            // Suche alle Feinde mit Tag "Enemy"
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            
            foreach (GameObject enemy in enemies)
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                
                // Prüfe ob in Reichweite und näher als bisheriger nächster Feind
                if (distance <= detectionRange && distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestEnemy = enemy;
                }
            }
            
            return nearestEnemy;
        }

        void Shoot(Vector3 direction)
        {
            Vector3 spawnPos = firePoint ? firePoint.position : transform.position + direction;
            
            // Bullet vom PoolManager holen
            GameObject bullet = PoolManager.Instance.Spawn("Bullet", spawnPos, Quaternion.LookRotation(direction));
            
            if (bullet == null)
            {
                Debug.LogWarning("Could not spawn bullet from pool!");
                return;
            }
            
            // Geschwindigkeit in Richtung des Feindes setzen
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = direction * bulletSpeed;
            }
        }
        
        // Optional: Visualisierung der Reichweite im Editor
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);
        }
    }
}