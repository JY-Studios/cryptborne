using UnityEngine;
using UnityEngine.Serialization;

namespace Weapons.Data
{
    [CreateAssetMenu(menuName = "Weapons/MeleeWeapon")]
    public class MeleeWeaponData : WeaponData
    {
        public float radius;
        public float arcAngle;
    }
}