using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WaveSurvival.UI
{
    /// <summary>Shop panel. Creates rows on request and forwards button clicks as events.</summary>
    public class ShopView : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text goldText;
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private Transform itemContainer;
        [SerializeField] private ShopItemView itemPrefab;
        [SerializeField] private Button medkitButton;
        [SerializeField] private TMP_Text medkitLabel;
        [SerializeField] private Button startWaveButton;
        [SerializeField] private Button closeButton;

        private readonly List<ShopItemView> _items = new();

        public event Action<int> ItemClicked;
        public event Action MedkitClicked;
        public event Action StartWaveClicked;
        public event Action CloseClicked;

        public bool IsVisible => root.activeSelf;

        private void Awake()
        {
            medkitButton.onClick.AddListener(() => MedkitClicked?.Invoke());
            startWaveButton.onClick.AddListener(() => StartWaveClicked?.Invoke());
            closeButton.onClick.AddListener(() => CloseClicked?.Invoke());
            root.SetActive(false);
        }

        public void SetVisible(bool visible)
        {
            root.SetActive(visible);
            if (visible) messageText.text = string.Empty;
        }

        public void SetGold(int gold) => goldText.text = $"Gold: {gold}";
        public void SetMessage(string message) => messageText.text = message;

        public void SetMedkit(int price, bool interactable)
        {
            medkitLabel.text = $"Medkit (full heal)  {price} G";
            medkitButton.interactable = interactable;
        }

        public ShopItemView GetOrCreateItem(int index)
        {
            while (_items.Count <= index)
            {
                ShopItemView item = Instantiate(itemPrefab, itemContainer);
                int captured = _items.Count;
                item.Clicked += () => ItemClicked?.Invoke(captured);
                _items.Add(item);
            }
            return _items[index];
        }
    }
}
