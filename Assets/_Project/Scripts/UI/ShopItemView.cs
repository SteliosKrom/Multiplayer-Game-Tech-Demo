using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WaveSurvival.UI
{
    /// <summary>One row in the shop: name, stats and an action button.</summary>
    public class ShopItemView : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text statsText;
        [SerializeField] private Button button;
        [SerializeField] private TMP_Text buttonLabel;

        public event Action Clicked;

        private void Awake() => button.onClick.AddListener(() => Clicked?.Invoke());

        public void Bind(string displayName, string stats, string actionLabel, bool interactable)
        {
            nameText.text = displayName;
            statsText.text = stats;
            buttonLabel.text = actionLabel;
            button.interactable = interactable;
        }
    }
}
