using System.Collections.Generic;
using UnityEngine;
using Weapons.Projectiles;

namespace Weapons.Data
{
    public class WeaponData : ScriptableObject
    {
        public string weaponName;
        public Sprite icon;

        [Header("Stats")]
        public float damage;
        public float attackSpeed;

        [Header("Visuals")]
        public GameObject weaponPrefab;
    }
}