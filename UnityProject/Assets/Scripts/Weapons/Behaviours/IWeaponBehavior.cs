using UnityEngine;
using Weapons.Data;

namespace Weapons.Behaviours
{
    public interface IWeaponBehavior<in TData>
        where TData : WeaponData
    {
        /// <summary>
        /// Führt einen Angriff aus.
        /// </summary>
        /// <returns>True wenn tatsächlich angegriffen wurde, false wenn nicht (z.B. kein Target)</returns>
        bool Attack(TData data, Transform player);
    }
}