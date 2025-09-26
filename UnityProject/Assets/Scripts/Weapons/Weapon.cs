using UnityEngine;
using Weapons.Behaviours;
using Weapons.Data;

namespace Weapons
{
    public class Weapon<TData>
        where TData : WeaponData
    {
        private readonly TData _data;
        private IWeaponBehavior<TData> _behavior;
        private float _lastAttackTime = -999f;
        
        public Weapon(TData data, IWeaponBehavior<TData> behavior)
        {
            _data = data;
            _behavior = behavior;
        }
        
        public void Attack(Vector3 pos, Vector3 dir)
        {
            if (Time.time < _lastAttackTime + 1f / _data.attackSpeed) return;
            _behavior.Attack(_data, pos, dir); 
        }
    }
}