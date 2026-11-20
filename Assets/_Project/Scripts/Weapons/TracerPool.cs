using System.Collections.Generic;
using UnityEngine;

namespace WaveSurvival.Weapons
{
    /// <summary>Recycles short-lived LineRenderers for bullet tracers. Visual only.</summary>
    public class TracerPool : MonoBehaviour
    {
        [SerializeField] private LineRenderer tracerPrefab;
        [SerializeField] private float lifetime = 0.06f;

        private readonly Queue<LineRenderer> _free = new();
        private readonly List<(LineRenderer line, float until)> _active = new();

        public void Show(Vector3 from, Vector3 to, Color color)
        {
            var line = _free.Count > 0 ? _free.Dequeue() : Instantiate(tracerPrefab, transform);
            line.gameObject.SetActive(true);
            line.SetPosition(0, from);
            line.SetPosition(1, to);
            line.startColor = color;
            line.endColor = new Color(color.r, color.g, color.b, 0.2f);
            _active.Add((line, Time.time + lifetime));
        }

        private void Update()
        {
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                if (Time.time < _active[i].until) continue;
                _active[i].line.gameObject.SetActive(false);
                _free.Enqueue(_active[i].line);
                _active.RemoveAt(i);
            }
        }
    }
}
