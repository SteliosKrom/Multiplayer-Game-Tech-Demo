using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WaveSurvival.UI
{
    /// <summary>Passive view: only setters, no game logic. The presenter decides what to show.</summary>
    public class HudView : MonoBehaviour
    {
        [SerializeField] private Image healthFill;
        [SerializeField] private TMP_Text healthText;
        [SerializeField] private Image xpFill;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text goldText;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text waveText;
        [SerializeField] private TMP_Text enemiesText;
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private TMP_Text weaponText;
        [SerializeField] private TMP_Text hintText;
        [SerializeField] private TMP_Text toastText;

        private Coroutine _toast;

        public void SetHealth(float current, float max)
        {
            healthFill.fillAmount = max > 0f ? current / max : 0f;
            healthText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
        }

        public void SetXp(int level, int xp, int xpToNext)
        {
            xpFill.fillAmount = xpToNext > 0 ? (float)xp / xpToNext : 0f;
            levelText.text = $"LV {level}  <size=70%>{xp}/{xpToNext} XP</size>";
        }

        public void SetGold(int gold) => goldText.text = $"{gold} G";
        public void SetScore(int score) => scoreText.text = $"SCORE {score}";
        public void SetWave(int wave) => waveText.text = wave > 0 ? $"WAVE {wave}" : "GET READY";
        public void SetEnemiesRemaining(int count) => enemiesText.text = $"Enemies: {count}";
        public void SetWeapon(string weaponName) => weaponText.text = weaponName;
        public void SetHint(string hint) => hintText.text = hint;

        public void SetBreakTimer(float seconds)
        {
            bool visible = seconds > 0f;
            timerText.gameObject.SetActive(visible);
            if (visible) timerText.text = $"Next wave in {Mathf.CeilToInt(seconds)}";
        }

        public void ShowToast(string message)
        {
            if (_toast != null) StopCoroutine(_toast);
            _toast = StartCoroutine(ToastRoutine(message));
        }

        private IEnumerator ToastRoutine(string message)
        {
            toastText.text = message;
            toastText.gameObject.SetActive(true);
            yield return new WaitForSeconds(1.8f);
            toastText.gameObject.SetActive(false);
        }
    }
}
