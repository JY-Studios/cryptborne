using System.Collections.Generic;
using UnityEngine;
using Weapons.Projectiles;

namespace Weapons.Data
{
    [CreateAssetMenu(menuName = "Weapons/RangedWeapon")]
    public class RangedWeaponData : WeaponData
    {
        public float range;
        public List<ProjectileConfig> projectiles;
    }
}