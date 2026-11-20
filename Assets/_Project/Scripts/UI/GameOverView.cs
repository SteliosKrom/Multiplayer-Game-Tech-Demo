using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WaveSurvival.UI
{
    public class GameOverView : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text summaryText;
        [SerializeField] private TMP_Text highScoreText;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button menuButton;

        public event Action RetryClicked;
        public event Action MenuClicked;

        private void Awake()
        {
            retryButton.onClick.AddListener(() => RetryClicked?.Invoke());
            menuButton.onClick.AddListener(() => MenuClicked?.Invoke());
            root.SetActive(false);
        }

        public void Show(int score, int wave, int kills, int highScore, bool isNewHighScore)
        {
            summaryText.text = $"Score: {score}\nWave reached: {wave}\nKills: {kills}";
            highScoreText.text = isNewHighScore ? "NEW HIGH SCORE!" : $"High score: {highScore}";
            root.SetActive(true);
        }
    }
}
