using Characters.Enemies;
using UnityEngine;

namespace Weapons.Projectiles.Effects
{
    public class DamageEffect : IProjectileEffect
    {
        private int damage; // Integer statt float

        public int Damage => damage; // Integer Property

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