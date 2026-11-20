using System.Collections.Generic;
using UnityEngine;

namespace WaveSurvival.Weapons
{
    /// <summary>The list of all weapons in the game and lookup by id.</summary>
    [CreateAssetMenu(menuName = "Wave Survival/Weapon Catalog")]
    public class WeaponCatalog : ScriptableObject
    {
        [SerializeField] private List<WeaponDefinition> weapons = new();

        public IReadOnlyList<WeaponDefinition> All => weapons;

        public WeaponDefinition Find(string id)
        {
            foreach (var weapon in weapons)
                if (weapon != null && weapon.id == id) return weapon;
            return null;
        }

        public int IndexOf(string id)
        {
            for (int i = 0; i < weapons.Count; i++)
                if (weapons[i] != null && weapons[i].id == id) return i;
            return -1;
        }

        public IEnumerable<string> DefaultOwnedIds()
        {
            foreach (var weapon in weapons)
                if (weapon != null && weapon.ownedByDefault) yield return weapon.id;
        }

        public void SetWeapons(List<WeaponDefinition> list) => weapons = list;
    }
}
