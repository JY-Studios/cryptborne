using Characters.Enemies;
using UnityEngine;

namespace Weapons.Projectiles.Effects
{
    public class DamageEffect : IProjectileEffect
    {
        private float damage;

        public DamageEffect(float damage)
        {
            this.damage = damage;
        }

        public void OnHit(GameObject target, Projectile projectile)
        {
            if (!target.CompareTag("Enemy")) return;

            var health = target.GetComponent<EnemyHealth>();
            if (health != null)
                health.TakeDamage(damage);
        }
    }
}