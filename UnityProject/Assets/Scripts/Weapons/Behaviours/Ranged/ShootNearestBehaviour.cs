using System.Linq;
using Characters.Enemies;
using UnityEngine;
using Weapons.Data;
using Weapons.Projectiles;
using Weapons.Projectiles.Effects;
using Weapons.VFX;
using Weapons.Audio;

namespace Weapons.Behaviours.Ranged
{
    public class ShootNearestBehaviour : IWeaponBehavior<RangedWeaponData>
    {
        private float _nextShootTime = 0f;

        public void Attack(RangedWeaponData data, Transform player)
        {
            // Prüfe ob es Orbit-Projektile gibt - diese brauchen KEIN Target!
            bool hasOrbitProjectiles = data.projectiles.Any(p => p.pattern == SpreadPattern.Orbit);
            
            if (hasOrbitProjectiles)
            {
                // Orbit-Waffen spawnen IMMER, unabhängig von Enemies
                foreach (var projectile in data.projectiles)
                {
                    if (projectile.pattern == SpreadPattern.Orbit)
                    {
                        SpawnProjectiles(projectile, player, Vector3.forward, data.damage);
                    }
                }
                return;
            }
            
            // Normale Waffen brauchen ein Target
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

            foreach (var projectile in data.projectiles)
            {
                SpawnProjectiles(projectile, player, dir, data.damage);
            }
        }

        private void SpawnProjectiles(ProjectileConfig config, Transform player, Vector3 baseDir, float baseDamage)
        {
            // Spezielle Behandlung für Orbit-Pattern
            if (config.pattern == SpreadPattern.Orbit)
            {
                SpawnOrbitProjectiles(config, player, baseDamage);
                return;
            }
            
            // Normale Projektile
            Vector3 baseSpawnPos = player.position + baseDir;
            
            // MUZZLE FLASH und SOUND spawnen beim Schießen
            VFXManager.SpawnMuzzleFlash(baseSpawnPos, baseDir);
            SoundManager.Instance.PlayShootSound();
            
            Vector3[] directions;
            Vector3[] spawnPositions;
            
            CalculateSpreadPattern(baseDir, baseSpawnPos, config, out directions, out spawnPositions);
            
            for (int i = 0; i < directions.Length; i++)
            {
                var go = PoolManager.Instance.Spawn(
                    poolName: config.prefab.name,
                    position: spawnPositions[i],
                    rotation: Quaternion.LookRotation(directions[i]),
                    fallbackPrefab: config.prefab,
                    defaultSize: 30
                );

                var proj = go.GetComponent<Projectile>();
                proj.ResetProjectile();
                proj.Init(config, directions[i], new DamageEffect(baseDamage));
            }
        }
        
        private void SpawnOrbitProjectiles(ProjectileConfig config, Transform player, float baseDamage)
        {
            float orbitStep = 360f / config.count;
            
            // Spawn-Effekt und Sound in der Mitte (nur einmal für alle Orbit-Projektile)
            VFXManager.SpawnMuzzleFlash(player.position, player.forward);
            SoundManager.Instance.PlayShootSound();
            
            for (int i = 0; i < config.count; i++)
            {
                float startAngle = (orbitStep * i) + config.orbitStartAngle;
                float angleRad = startAngle * Mathf.Deg2Rad;
                
                Vector3 offset = new Vector3(
                    Mathf.Cos(angleRad) * config.orbitRadius,
                    0f,
                    Mathf.Sin(angleRad) * config.orbitRadius
                );
                Vector3 spawnPos = player.position + offset;
                
                float tangentAngle = startAngle + 90f;
                Quaternion rotation = Quaternion.Euler(0f, tangentAngle, 0f);
                
                var go = PoolManager.Instance.Spawn(
                    poolName: config.prefab.name,
                    position: spawnPos,
                    rotation: rotation,
                    fallbackPrefab: config.prefab,
                    defaultSize: 30
                );

                var proj = go.GetComponent<Projectile>();
                proj.ResetProjectile();
                proj.InitOrbit(config, startAngle, player, new DamageEffect(baseDamage));
            }
        }

        private void CalculateSpreadPattern(Vector3 baseDir, Vector3 baseSpawnPos, ProjectileConfig config, 
            out Vector3[] directions, out Vector3[] spawnPositions)
        {
            directions = new Vector3[config.count];
            spawnPositions = new Vector3[config.count];
            
            switch (config.pattern)
            {
                case SpreadPattern.None:
                    for (int i = 0; i < config.count; i++)
                    {
                        directions[i] = baseDir;
                        spawnPositions[i] = baseSpawnPos;
                    }
                    break;
                
                case SpreadPattern.Random:
                    for (int i = 0; i < config.count; i++)
                    {
                        float angleY = Random.Range(-config.spread * 0.5f, config.spread * 0.5f);
                        float angleX = Random.Range(-config.spread * 0.25f, config.spread * 0.25f);
                        Quaternion rot = Quaternion.Euler(angleX, angleY, 0);
                        directions[i] = rot * baseDir;
                        spawnPositions[i] = baseSpawnPos;
                    }
                    break;
                    
                case SpreadPattern.Cone:
                    float angleStep = config.count > 1 
                        ? config.spread / (config.count - 1) 
                        : 0;
                    float startAngle = -config.spread * 0.5f;
                    
                    for (int i = 0; i < config.count; i++)
                    {
                        float angle = startAngle + (angleStep * i);
                        angle += Random.Range(-1f, 1f);
                        Quaternion rot = Quaternion.Euler(0, angle, 0);
                        directions[i] = rot * baseDir;
                        spawnPositions[i] = baseSpawnPos;
                    }
                    break;
                    
                case SpreadPattern.Horizontal:
                    float hStep = config.count > 1 
                        ? config.spread / (config.count - 1) 
                        : 0;
                    float hStart = -config.spread * 0.5f;
                    
                    for (int i = 0; i < config.count; i++)
                    {
                        float angle = hStart + (hStep * i);
                        Quaternion rot = Quaternion.Euler(0, angle, 0);
                        directions[i] = rot * baseDir;
                        spawnPositions[i] = baseSpawnPos;
                    }
                    break;
                    
                case SpreadPattern.Vertical:
                    float vStep = config.count > 1 
                        ? config.spread / (config.count - 1) 
                        : 0;
                    float vStart = -config.spread * 0.5f;
                    
                    for (int i = 0; i < config.count; i++)
                    {
                        float angle = vStart + (vStep * i);
                        Quaternion rot = Quaternion.Euler(angle, 0, 0);
                        directions[i] = rot * baseDir;
                        spawnPositions[i] = baseSpawnPos;
                    }
                    break;
                    
                case SpreadPattern.Radial:
                    float radialStep = 360f / config.count;
                    
                    for (int i = 0; i < config.count; i++)
                    {
                        float angle = radialStep * i;
                        Quaternion rot = Quaternion.Euler(0, angle, 0);
                        directions[i] = rot * Vector3.forward;
                        spawnPositions[i] = baseSpawnPos;
                    }
                    break;
                    
                case SpreadPattern.Orbit:
                    Debug.LogWarning("Orbit pattern should be handled separately!");
                    for (int i = 0; i < config.count; i++)
                    {
                        directions[i] = baseDir;
                        spawnPositions[i] = baseSpawnPos;
                    }
                    break;
                    
                case SpreadPattern.Spiral:
                    float spiralStep = 360f / config.count;
                    float spiralSpread = config.spread / config.count;
                    
                    for (int i = 0; i < config.count; i++)
                    {
                        float angle = spiralStep * i;
                        float currentSpread = spiralSpread * i;
                        
                        Quaternion rot = Quaternion.Euler(0, angle, 0);
                        Vector3 spiralDir = rot * baseDir;
                        
                        Quaternion spreadRot = Quaternion.Euler(0, currentSpread, 0);
                        directions[i] = spreadRot * spiralDir;
                        spawnPositions[i] = baseSpawnPos;
                    }
                    break;
                    
                default:
                    for (int i = 0; i < config.count; i++)
                    {
                        directions[i] = baseDir;
                        spawnPositions[i] = baseSpawnPos;
                    }
                    break;
            }
        }
    }
}