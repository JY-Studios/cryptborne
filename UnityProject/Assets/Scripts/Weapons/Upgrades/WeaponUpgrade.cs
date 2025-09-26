using Weapons.Data;

namespace Weapons.Upgrades
{
    public abstract class WeaponUpgrade
    {
        public abstract void Apply(WeaponData data);
    }
}