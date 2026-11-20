using System;
using UnityEngine;
using WaveSurvival.Progression;

namespace WaveSurvival.UI
{
    public sealed class MainMenuPresenter : IDisposable
    {
        private readonly MainMenuView _view;

        public MainMenuPresenter(MainMenuView view, PlayerProfile profile, bool multiplayerAvailable)
        {
            _view = view;
            _view.SetProfile(profile.level, profile.gold, profile.highScore, profile.bestWave, profile.ownedWeapons.Count);
            _view.SetMultiplayerAvailable(multiplayerAvailable);

            _view.PlayClicked += OnPlay;
            _view.QuitClicked += OnQuit;
        }

        public void Dispose()
        {
            _view.PlayClicked -= OnPlay;
            _view.QuitClicked -= OnQuit;
        }

        private static void OnPlay() => SceneLoader.Load(SceneNames.Game);

        private static void OnQuit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
