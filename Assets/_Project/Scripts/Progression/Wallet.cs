using System;

namespace WaveSurvival.Progression
{
    public interface IWallet
    {
        int Gold { get; }
        event Action<int> GoldChanged;
        void Add(int amount);
        bool TrySpend(int amount);
    }

    /// <summary>Gold only. Spending fails instead of going negative.</summary>
    public sealed class Wallet : IWallet
    {
        private readonly PlayerProfile _profile;

        public Wallet(PlayerProfile profile) => _profile = profile;

        public int Gold => _profile.gold;
        public event Action<int> GoldChanged;

        public void Add(int amount)
        {
            if (amount <= 0) return;
            _profile.gold += amount;
            GoldChanged?.Invoke(_profile.gold);
        }

        public bool TrySpend(int amount)
        {
            if (amount < 0 || _profile.gold < amount) return false;
            _profile.gold -= amount;
            GoldChanged?.Invoke(_profile.gold);
            return true;
        }
    }
}
