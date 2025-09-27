using UnityEngine;
using Weapons.Data;

namespace Weapons.Behaviours
{
    public interface IWeaponBehavior<in TData>
        where TData : WeaponData
    {
        void Attack(TData data, Transform player);
    }
}