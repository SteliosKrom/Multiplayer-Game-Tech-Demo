using UnityEngine;
using WaveSurvival.Persistence;
using WaveSurvival.UI;
using WaveSurvival.Weapons;

namespace WaveSurvival.Bootstrap
{
    /// <summary>Composition root of the MainMenu scene.</summary>
    public class MainMenuInstaller : MonoBehaviour
    {
        [SerializeField] private MainMenuView view;
        [SerializeField] private WeaponCatalog weaponCatalog;
        [Tooltip("Singleplayer build: multiplayer is shown but disabled with a COMING SOON label.")]
        [SerializeField] private bool multiplayerAvailable;

        private MainMenuPresenter _presenter;

        private async void Start()
        {
            var session = new ProfileSession(new JsonFileProfileRepository());
            await session.LoadAsync();
            if (this == null) return;

            // Make sure default weapons show up in the summary even before the first run.
            new Progression.WeaponInventory(session.Profile, weaponCatalog.DefaultOwnedIds());
            _presenter = new MainMenuPresenter(view, session.Profile, multiplayerAvailable);
        }

        private void OnDestroy() => _presenter?.Dispose();
    }
}
