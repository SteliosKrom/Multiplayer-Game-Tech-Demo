using UnityEngine;
using WaveSurvival.Core;

namespace WaveSurvival.Combat
{
    /// <summary>Purely visual: tints the renderers white for a moment when the Health is damaged.</summary>
    [RequireComponent(typeof(Health))]
    public class HitFlash : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        [SerializeField] private Renderer[] renderers;
        [SerializeField] private Color flashColor = Color.white;
        [SerializeField] private float duration = 0.08f;

        private MaterialPropertyBlock _block;
        private Color[] _originalColors;
        private float _timer;
        private Health _health;

        private void Awake()
        {
            _health = GetComponent<Health>();
            _block = new MaterialPropertyBlock();
            if (renderers == null || renderers.Length == 0) renderers = GetComponentsInChildren<Renderer>();

            _originalColors = new Color[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
                _originalColors[i] = renderers[i].sharedMaterial.GetColor(BaseColorId);
        }

        private void OnEnable() => _health.Damaged += OnDamaged;
        private void OnDisable() => _health.Damaged -= OnDamaged;

        private void OnDamaged(DamageInfo _)
        {
            _timer = duration;
            Apply(flashColor, useOriginal: false);
        }

        private void Update()
        {
            if (_timer <= 0f) return;
            _timer -= Time.deltaTime;
            if (_timer <= 0f) Apply(Color.clear, useOriginal: true);
        }

        private void Apply(Color color, bool useOriginal)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].GetPropertyBlock(_block);
                _block.SetColor(BaseColorId, useOriginal ? _originalColors[i] : color);
                renderers[i].SetPropertyBlock(_block);
            }
        }
    }
}
