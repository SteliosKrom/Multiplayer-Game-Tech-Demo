using System;
using UnityEngine;
using WaveSurvival.Input;
using WaveSurvival.Persistence;

namespace WaveSurvival.UI
{
    /// <summary>Esc toggles pause (singleplayer freezes time). Leaving to the menu saves first.</summary>
    public sealed class PausePresenter : ITickable, IDisposable
    {
        private readonly PauseView _view;
        private readonly IPlayerInput _input;
        private readonly UiState _ui;
        private readonly ProfileSession _profile;

        public PausePresenter(PauseView view, IPlayerInput input, UiState ui, ProfileSession profile)
        {
            _view = view;
            _input = input;
            _ui = ui;
            _profile = profile;

            _view.ResumeClicked += Resume;
            _view.MenuClicked += OnMenu;
        }

        public void Dispose()
        {
            _view.ResumeClicked -= Resume;
            _view.MenuClicked -= OnMenu;
        }

        public void Tick()
        {
            if (!_input.PausePressed || _ui.GameOver || _ui.ShopOpen) return;
            if (_ui.Paused) Resume();
            else Pause();
        }

        private void Pause()
        {
            _ui.Paused = true;
            Time.timeScale = 0f;
            _view.SetVisible(true);
        }

        private void Resume()
        {
            _ui.Paused = false;
            Time.timeScale = 1f;
            _view.SetVisible(false);
        }

        private async void OnMenu()
        {
            await _profile.SaveAsync();
            SceneLoader.Load(SceneNames.MainMenu);
        }
    }
}
