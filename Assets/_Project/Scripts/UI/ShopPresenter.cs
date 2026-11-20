using System;
using WaveSurvival.Combat;
using WaveSurvival.Input;
using WaveSurvival.Persistence;
using WaveSurvival.Progression;
using WaveSurvival.Waves;
using WaveSurvival.Weapons;

namespace WaveSurvival.UI
{
    /// <summary>
    /// Opens/closes the shop between waves, renders the catalog and turns clicks into ShopService calls.
    /// Purchases are saved immediately so they are never lost.
    /// </summary>
    public sealed class ShopPresenter : ITickable, IDisposable
    {
        private readonly ShopView _view;
        private readonly ShopService _shop;
        private readonly WeaponCatalog _catalog;
        private readonly IWeaponInventory _inventory;
        private readonly IWallet _wallet;
        private readonly WeaponHolder _weapons;
        private readonly Health _health;
        private readonly WaveDirector _waves;
        private readonly UiState _ui;
        private readonly IPlayerInput _input;
        private readonly ProfileSession _profile;

        public ShopPresenter(ShopView view, ShopService shop, WeaponCatalog catalog, IWeaponInventory inventory,
            IWallet wallet, WeaponHolder weapons, Health health, WaveDirector waves, UiState ui,
            IPlayerInput input, ProfileSession profile)
        {
            _view = view;
            _shop = shop;
            _catalog = catalog;
            _inventory = inventory;
            _wallet = wallet;
            _weapons = weapons;
            _health = health;
            _waves = waves;
            _ui = ui;
            _input = input;
            _profile = profile;

            _view.ItemClicked += OnItemClicked;
            _view.MedkitClicked += OnMedkitClicked;
            _view.StartWaveClicked += OnStartWaveClicked;
            _view.CloseClicked += Close;
            _waves.WaveStarted += OnWaveStarted;
            _wallet.GoldChanged += OnGoldChanged;
        }

        public void Dispose()
        {
            _view.ItemClicked -= OnItemClicked;
            _view.MedkitClicked -= OnMedkitClicked;
            _view.StartWaveClicked -= OnStartWaveClicked;
            _view.CloseClicked -= Close;
            _waves.WaveStarted -= OnWaveStarted;
            _wallet.GoldChanged -= OnGoldChanged;
        }

        public void Tick()
        {
            if (!_input.ShopPressed || _ui.Paused || _ui.GameOver) return;
            if (_ui.ShopOpen) Close();
            else if (_waves.Phase == WavePhase.Break) Open();
        }

        private void Open()
        {
            _ui.ShopOpen = true;
            _view.SetVisible(true);
            Refresh();
        }

        private void Close()
        {
            _ui.ShopOpen = false;
            _view.SetVisible(false);
        }

        private void Refresh()
        {
            _view.SetGold(_wallet.Gold);
            _view.SetMedkit(_shop.MedkitPrice, _wallet.Gold >= _shop.MedkitPrice && _health.Current < _health.Max);

            for (int i = 0; i < _catalog.All.Count; i++)
            {
                WeaponDefinition weapon = _catalog.All[i];
                bool owned = _inventory.Owns(weapon.id);
                bool equipped = _weapons.Current == weapon;

                string label = equipped ? "Equipped" : owned ? "Equip" : $"Buy {weapon.price} G";
                bool interactable = !equipped && (owned || _wallet.Gold >= weapon.price);
                string stats = $"[{i + 1}]  DMG {weapon.damage:0} x{weapon.pellets}   RPS {weapon.fireRate:0.#}   RNG {weapon.range:0}";

                _view.GetOrCreateItem(i).Bind(weapon.displayName, stats, label, interactable);
            }
        }

        private void OnItemClicked(int index)
        {
            WeaponDefinition weapon = _catalog.All[index];

            if (!_inventory.Owns(weapon.id))
            {
                PurchaseResult result = _shop.TryBuyWeapon(weapon);
                _view.SetMessage(result == PurchaseResult.Success ? $"Bought {weapon.displayName}!" : result.ToString());
                if (result != PurchaseResult.Success) return;
                _profile.SaveInBackground();
            }

            _weapons.Equip(weapon);
            Refresh();
        }

        private void OnMedkitClicked()
        {
            PurchaseResult result = _shop.TryBuyMedkit(_health);
            _view.SetMessage(result == PurchaseResult.Success ? "Healed!" : result.ToString());
            Refresh();
        }

        private void OnStartWaveClicked()
        {
            Close();
            _waves.SkipBreak();
        }

        private void OnWaveStarted(int _) => Close();

        private void OnGoldChanged(int _)
        {
            if (_ui.ShopOpen) Refresh();
        }
    }
}
