using Characters.Enemies;
using UnityEngine;

namespace Weapons.Projectiles.Effects
{
    public class DamageEffect : IProjectileEffect
    {
        private int damage;

        public DamageEffect(int damage)
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