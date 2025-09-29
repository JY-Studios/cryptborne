using System.Linq;
using Characters.Enemies;
using UnityEngine;
using Weapons.Data;
using Weapons.Projectiles;
using Weapons.Projectiles.Effects;

namespace Weapons.Behaviours.Ranged
{
    public class ShootNearestBehaviour : IWeaponBehavior<RangedWeaponData>
    {
        private float _nextShootTime = 0f;

        public void Attack(RangedWeaponData data, Transform player)
        {
            var nearestEnemy = EnemySpawner.Instance.ActiveEnemies
                .Where(e => e && e.activeInHierarchy)
                .Select(e => new { enemy = e, dist = Vector3.Distance(player.position, e.transform.position) })
                .Where(x => x.dist <= data.detectionRange)
                .OrderBy(x => x.dist)
                .Select(x => x.enemy)
                .FirstOrDefault();
            
            if (!nearestEnemy)
                return;
            
            Vector3 dir = (nearestEnemy.transform.position - player.position).normalized;
            dir.y = 0f;
            player.rotation = Quaternion.LookRotation(dir);
            
            Vector3 spawnPos = player.position + dir;

            foreach (var projectile in data.projectiles)
            {
                Vector3[] directions = CalculateSpreadPattern(dir, projectile);
                
                for (int i = 0; i < directions.Length; i++)
                {
                    var go = PoolManager.Instance.Spawn(
                        poolName: projectile.prefab.name,
                        position: spawnPos,
                        rotation: Quaternion.LookRotation(directions[i]),
                        fallbackPrefab: projectile.prefab,
                        defaultSize: 30
                    );

                    var proj = go.GetComponent<Projectile>();
                    proj.ResetProjectile();
                    
                    // Schaden anpassen basierend auf Anzahl der Projektile
                    float adjustedDamage = CalculateProjectileDamage(data.damage, projectile);
                    proj.Init(projectile, directions[i], new DamageEffect(adjustedDamage));
                }
            }
        }

        private Vector3[] CalculateSpreadPattern(Vector3 baseDir, ProjectileConfig config)
        {
            Vector3[] directions = new Vector3[config.count];
            
            switch (config.pattern)
            {
                case SpreadPattern.Cone:
                    // Gleichmäßiger Kegel (perfekt für Shotgun)
                    float angleStep = config.count > 1 
                        ? config.spread / (config.count - 1) 
                        : 0;
                    float startAngle = -config.spread * 0.5f;
                    
                    for (int i = 0; i < config.count; i++)
                    {
                        float angle = startAngle + (angleStep * i);
                        angle += Random.Range(-1f, 1f); // Kleine Variation für Realismus
                        Quaternion rot = Quaternion.Euler(0, angle, 0);
                        directions[i] = rot * baseDir;
                    }
                    break;
                    
                case SpreadPattern.Horizontal:
                    // Horizontale Linie
                    float hStep = config.count > 1 
                        ? config.spread / (config.count - 1) 
                        : 0;
                    float hStart = -config.spread * 0.5f;
                    
                    for (int i = 0; i < config.count; i++)
                    {
                        float angle = hStart + (hStep * i);
                        Quaternion rot = Quaternion.Euler(0, angle, 0);
                        directions[i] = rot * baseDir;
                    }
                    break;
                    
                case SpreadPattern.Vertical:
                    // Vertikale Linie
                    float vStep = config.count > 1 
                        ? config.spread / (config.count - 1) 
                        : 0;
                    float vStart = -config.spread * 0.5f;
                    
                    for (int i = 0; i < config.count; i++)
                    {
                        float angle = vStart + (vStep * i);
                        Quaternion rot = Quaternion.Euler(angle, 0, 0);
                        directions[i] = rot * baseDir;
                    }
                    break;
                    
                case SpreadPattern.Circle:
                    // Kreisförmig
                    float circleStep = 360f / config.count;
                    float radius = config.spread * 0.5f;
                    
                    for (int i = 0; i < config.count; i++)
                    {
                        float angle = circleStep * i;
                        Quaternion rot = Quaternion.Euler(0, angle, 0);
                        Vector3 offset = rot * (Vector3.forward * radius * 0.1f);
                        directions[i] = (baseDir + offset).normalized;
                    }
                    break;
                    
                case SpreadPattern.Random:
                default:
                    // Zufällige Streuung (gut für Machinegun)
                    for (int i = 0; i < config.count; i++)
                    {
                        float angleY = Random.Range(-config.spread * 0.5f, config.spread * 0.5f);
                        float angleX = Random.Range(-config.spread * 0.25f, config.spread * 0.25f); // Leichte vertikale Streuung
                        Quaternion rot = Quaternion.Euler(angleX, angleY, 0);
                        directions[i] = rot * baseDir;
                    }
                    break;
            }
            
            return directions;
        }
        
        private float CalculateProjectileDamage(float baseDamage, ProjectileConfig config)
        {
            // Für Shotgun-artige Waffen: Schaden auf Pellets aufteilen
            // Aber nicht zu stark, damit einzelne Treffer noch Sinn machen
            switch (config.pattern)
            {
                case SpreadPattern.Cone:
                case SpreadPattern.Circle:
                    // Shotgun-Logik: Gesamtschaden wird etwas aufgeteilt
                    // Aber einzelne Pellets sollten noch Schaden machen
                    return baseDamage / Mathf.Max(1f, config.count * 0.3f);
                    
                case SpreadPattern.Horizontal:
                case SpreadPattern.Vertical:
                    // Mittlere Aufteilung
                    return baseDamage / Mathf.Max(1f, config.count * 0.5f);
                    
                case SpreadPattern.Random:
                default:
                    // Voller Schaden pro Projektil (Machinegun)
                    return baseDamage;
            }
        }
    }
}