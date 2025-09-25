using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShoot : MonoBehaviour
{
    [Header("Shooting")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 20f;
    public float fireRate = 0.3f;
    
    private float nextFireTime = 0f;
    
    void Update()
    {
        // NEUES Input System
        if (Mouse.current != null && Mouse.current.leftButton.isPressed && Time.time >= nextFireTime)
        {
            Debug.Log("Shooting!"); // Test-Ausgabe
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }
    
    void Shoot()
    {
        if (bulletPrefab == null) 
        {
            Debug.LogError("Bullet Prefab fehlt!");
            return;
        }
        
        // Spawn Position
        Vector3 spawnPos = firePoint ? firePoint.position : transform.position + Vector3.forward;
        
        Debug.Log($"Spawning bullet at {spawnPos}");
        
        GameObject bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
        
        // Kugel größer machen zum Testen
        bullet.transform.localScale = Vector3.one * 0.5f;
        
        // Rigidbody setup
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb == null) rb = bullet.AddComponent<Rigidbody>();
        
        rb.useGravity = false;
        rb.linearVelocity = transform.forward * bulletSpeed;
        
        Destroy(bullet, 2f);
    }
}