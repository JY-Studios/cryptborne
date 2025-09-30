using UnityEngine;

namespace Weapons.Data
{
    public class WeaponData : ScriptableObject
    {
        public string weaponName;
        public Sprite icon;
        public FireMode fireMode;

        [Header("Stats")]
        public int damage; // Integer statt float
        public float attackSpeed;

        [Header("Visuals")]
        public GameObject weaponPrefab;
    }
}