using UnityEngine;
using UnityEngine.InputSystem;

namespace Characters.Player
{
    public class PlayerShoot : MonoBehaviour
    {
        [Header("Shooting")] public GameObject bulletPrefab;
        public Transform firePoint;
        public float bulletSpeed = 20f;
        public float fireRate = 0.3f;

        private float nextFireTime = 0f;

        void Update()
        {
            if (Mouse.current != null && Mouse.current.leftButton.isPressed && Time.time >= nextFireTime)
            {
                Shoot();
                nextFireTime = Time.time + fireRate;
            }
        }

        void Shoot()
        {
            if (bulletPrefab == null) return;

            Vector3 spawnPos = firePoint ? firePoint.position : transform.position + Vector3.forward;
            GameObject bullet = Instantiate(bulletPrefab, spawnPos, transform.rotation);
        
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = transform.forward * bulletSpeed;
            }

            Destroy(bullet, 2f);
        }
    }
}