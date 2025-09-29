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
        private float _lastAttackTime = float.MinValue;
        
        public Weapon(TData data, IWeaponBehavior<TData> behavior)
        {
            _data = data;
            _behavior = behavior;
            _lastAttackTime = Time.time - data.attackSpeed - 1f; // Sicherstellen dass Cooldown ready ist
        }
        
        private bool CooldownReady => Time.time >= _lastAttackTime + _data.attackSpeed;
        
        public void TryAttack(Transform player, bool inputPressed)
        {
            switch (_data.fireMode)
            {
                case FireMode.Manual:
                    if (inputPressed && CooldownReady) Attack(player);
                    break;

                case FireMode.AutoHold:
                    if (inputPressed && CooldownReady) Attack(player);
                    break;

                case FireMode.AutoFire:
                    if (CooldownReady) Attack(player);
                    break;
            }
        }

        private void Attack(Transform player)
        {
            _behavior.Attack(_data, player);
            _lastAttackTime = Time.time;
        }
    }
}