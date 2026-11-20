using System;
using System.Collections.Generic;

namespace WaveSurvival.Progression
{
    public interface IWeaponInventory
    {
        IReadOnlyList<string> OwnedIds { get; }
        event Action Changed;
        bool Owns(string weaponId);
        void Add(string weaponId);
    }

    /// <summary>Which weapon ids the player has unlocked. Stores ids, not ScriptableObjects, so it can be saved anywhere.</summary>
    public sealed class WeaponInventory : IWeaponInventory
    {
        private readonly PlayerProfile _profile;

        public WeaponInventory(PlayerProfile profile, IEnumerable<string> defaultIds)
        {
            _profile = profile;
            foreach (var id in defaultIds) AddSilently(id);
        }

        public IReadOnlyList<string> OwnedIds => _profile.ownedWeapons;
        public event Action Changed;

        public bool Owns(string weaponId) => _profile.ownedWeapons.Contains(weaponId);

        public void Add(string weaponId)
        {
            if (AddSilently(weaponId)) Changed?.Invoke();
        }

        private bool AddSilently(string weaponId)
        {
            if (string.IsNullOrEmpty(weaponId) || Owns(weaponId)) return false;
            _profile.ownedWeapons.Add(weaponId);
            return true;
        }
    }
}
