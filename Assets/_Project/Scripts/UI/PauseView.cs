using System;
using UnityEngine;
using UnityEngine.UI;

namespace WaveSurvival.UI
{
    public class PauseView : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button menuButton;

        public event Action ResumeClicked;
        public event Action MenuClicked;

        private void Awake()
        {
            resumeButton.onClick.AddListener(() => ResumeClicked?.Invoke());
            menuButton.onClick.AddListener(() => MenuClicked?.Invoke());
            root.SetActive(false);
        }

        public void SetVisible(bool visible) => root.SetActive(visible);
    }
}
