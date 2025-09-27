using UnityEngine;

namespace Weapons.Projectiles
{
    public interface IProjectileEffect
    {
        void OnHit(GameObject target, Projectile projectile);
    }
}