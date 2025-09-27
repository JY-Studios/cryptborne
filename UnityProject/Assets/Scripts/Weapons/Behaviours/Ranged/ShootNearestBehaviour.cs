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

            //Implement correct PoolManager
            foreach (var projectile in data.projectiles)
            {
                Quaternion spreadRot = Quaternion.Euler(0, Random.Range(-projectile.spread, projectile.spread), 0);
                Vector3 finalDir = spreadRot * dir;

                var go = PoolManager.Instance.Spawn(
                    poolName: projectile.prefab.name,     // z. B. "Bullet"
                    position: spawnPos,
                    rotation: Quaternion.LookRotation(finalDir),
                    fallbackPrefab: projectile.prefab,    // falls noch kein Pool existiert
                    defaultSize: 20                                 // Anfangsgröße
                );

                var proj = go.GetComponent<Projectile>();
                proj.ResetProjectile();
                proj.Init(projectile, finalDir, new DamageEffect((int)data.damage));
            }
        }
    }
}