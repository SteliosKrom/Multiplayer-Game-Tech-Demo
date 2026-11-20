using System;
using WaveSurvival.Bootstrap;

namespace WaveSurvival.UI
{
    public sealed class GameOverPresenter : IDisposable
    {
        private readonly GameOverView _view;
        private readonly GameSession _session;

        public GameOverPresenter(GameOverView view, GameSession session)
        {
            _view = view;
            _session = session;

            _session.Ended += OnEnded;
            _view.RetryClicked += OnRetry;
            _view.MenuClicked += OnMenu;
        }

        public void Dispose()
        {
            _session.Ended -= OnEnded;
            _view.RetryClicked -= OnRetry;
            _view.MenuClicked -= OnMenu;
        }

        private void OnEnded(RunResult result) =>
            _view.Show(result.Score, result.Wave, result.Kills, result.HighScore, result.IsNewHighScore);

        private static void OnRetry() => SceneLoader.Load(SceneNames.Game);
        private static void OnMenu() => SceneLoader.Load(SceneNames.MainMenu);
    }
}
