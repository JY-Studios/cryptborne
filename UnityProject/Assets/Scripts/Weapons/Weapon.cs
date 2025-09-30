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
            _lastAttackTime = Time.time - data.attackSpeed - 1f;
        }
        
        private bool CooldownReady => Time.time >= _lastAttackTime + _data.attackSpeed;
        
        /// <summary>
        /// Versucht zu attackieren. Gibt true zurück wenn Attack erfolgreich war.
        /// </summary>
        public bool TryAttack(Transform player, bool inputPressed)
        {
            bool shouldTryAttack = false;
            
            switch (_data.fireMode)
            {
                case FireMode.Manual:
                    shouldTryAttack = inputPressed && CooldownReady;
                    break;

                case FireMode.AutoHold:
                    shouldTryAttack = inputPressed && CooldownReady;
                    break;

                case FireMode.AutoFire:
                    shouldTryAttack = CooldownReady;
                    break;
            }
            
            if (shouldTryAttack)
            {
                bool didAttack = _behavior.Attack(_data, player);
                if (didAttack)
                {
                    _lastAttackTime = Time.time;
                    return true;
                }
            }
            
            return false;
        }
    }
}