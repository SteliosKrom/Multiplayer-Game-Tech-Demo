using UnityEngine;
using UnityEngine.SceneManagement;

namespace WaveSurvival.UI
{
    public static class SceneNames
    {
        public const string MainMenu = "MainMenu";
        public const string Game = "Game";
    }

    /// <summary>One place for scene changes, so time scale is always restored.</summary>
    public static class SceneLoader
    {
        public static void Load(string sceneName)
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(sceneName);
        }
    }
}
