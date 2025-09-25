using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShoot : MonoBehaviour
{
    [Header("Shooting")]
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
        Vector3 spawnPos = firePoint ? firePoint.position : transform.position + transform.forward;
        
        // Bullet vom PoolManager holen
        GameObject bullet = PoolManager.Instance.Spawn("Bullet", spawnPos, transform.rotation);
        
        if (bullet == null)
        {
            Debug.LogWarning("Could not spawn bullet from pool!");
            return;
        }
        
        // Geschwindigkeit setzen
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = transform.forward * bulletSpeed;
        }
    }
}