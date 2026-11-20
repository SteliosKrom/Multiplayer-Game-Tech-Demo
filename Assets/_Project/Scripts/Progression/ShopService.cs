using WaveSurvival.Combat;
using WaveSurvival.Weapons;

namespace WaveSurvival.Progression
{
    public enum PurchaseResult { Success, AlreadyOwned, NotEnoughGold, Invalid }

    /// <summary>Buying rules. Depends on abstractions (IWallet, IWeaponInventory), not on UI or save code.</summary>
    public sealed class ShopService
    {
        private readonly IWallet _wallet;
        private readonly IWeaponInventory _inventory;
        private readonly int _medkitPrice;

        public ShopService(IWallet wallet, IWeaponInventory inventory, int medkitPrice)
        {
            _wallet = wallet;
            _inventory = inventory;
            _medkitPrice = medkitPrice;
        }

        public int MedkitPrice => _medkitPrice;

        public PurchaseResult TryBuyWeapon(WeaponDefinition weapon)
        {
            if (weapon == null) return PurchaseResult.Invalid;
            if (_inventory.Owns(weapon.id)) return PurchaseResult.AlreadyOwned;
            if (!_wallet.TrySpend(weapon.price)) return PurchaseResult.NotEnoughGold;

            _inventory.Add(weapon.id);
            return PurchaseResult.Success;
        }

        public PurchaseResult TryBuyMedkit(Health health)
        {
            if (health == null || !health.IsAlive) return PurchaseResult.Invalid;
            if (health.Current >= health.Max) return PurchaseResult.AlreadyOwned;
            if (!_wallet.TrySpend(_medkitPrice)) return PurchaseResult.NotEnoughGold;

            health.Heal(health.Max);
            return PurchaseResult.Success;
        }
    }
}
