using WaveSurvival.Progression;
using WaveSurvival.Weapons;

namespace WaveSurvival.Player
{
    /// <summary>Maps number keys to weapons: slot i = catalog entry i, only if it is owned.</summary>
    public sealed class PlayerLoadout
    {
        private readonly WeaponCatalog _catalog;
        private readonly IWeaponInventory _inventory;

        public PlayerLoadout(WeaponCatalog catalog, IWeaponInventory inventory)
        {
            _catalog = catalog;
            _inventory = inventory;
        }

        public WeaponDefinition GetSlot(int slot)
        {
            if (slot < 0 || slot >= _catalog.All.Count) return null;
            WeaponDefinition weapon = _catalog.All[slot];
            return weapon != null && _inventory.Owns(weapon.id) ? weapon : null;
        }

        /// <summary>The best owned weapon (last in catalog order). Used when a run starts.</summary>
        public WeaponDefinition BestOwned()
        {
            for (int i = _catalog.All.Count - 1; i >= 0; i--)
            {
                WeaponDefinition weapon = GetSlot(i);
                if (weapon != null) return weapon;
            }
            return null;
        }
    }
}
