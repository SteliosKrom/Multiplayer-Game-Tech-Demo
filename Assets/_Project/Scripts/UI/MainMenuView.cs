using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WaveSurvival.UI
{
    public class MainMenuView : MonoBehaviour
    {
        [SerializeField] private Button playButton;
        [SerializeField] private Button multiplayerButton;
        [SerializeField] private GameObject comingSoonLabel;
        [SerializeField] private Button quitButton;
        [SerializeField] private TMP_Text profileText;

        public event Action PlayClicked;
        public event Action MultiplayerClicked;
        public event Action QuitClicked;

        private void Awake()
        {
            playButton.onClick.AddListener(() => PlayClicked?.Invoke());
            multiplayerButton.onClick.AddListener(() => MultiplayerClicked?.Invoke());
            quitButton.onClick.AddListener(() => QuitClicked?.Invoke());
        }

        public void SetMultiplayerAvailable(bool available)
        {
            multiplayerButton.interactable = available;
            comingSoonLabel.SetActive(!available);
        }

        public void SetProfile(int level, int gold, int highScore, int bestWave, int weaponsOwned)
        {
            profileText.text =
                $"Level {level}   |   {gold} Gold   |   {weaponsOwned} weapons\nHigh score {highScore}   |   Best wave {bestWave}";
        }
    }
}
