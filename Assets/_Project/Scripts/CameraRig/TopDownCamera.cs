using UnityEngine;

namespace WaveSurvival.CameraRig
{
    /// <summary>Follows a target from a fixed angled offset (top-down / third person hybrid).</summary>
    public class TopDownCamera : MonoBehaviour
    {
        [SerializeField] private Vector3 offset = new Vector3(0f, 19f, -13f);
        [SerializeField] private float smoothTime = 0.12f;

        private Transform _target;
        private Vector3 _velocity;

        public void SetTarget(Transform target)
        {
            _target = target;
            if (_target == null) return;
            transform.position = _target.position + offset;
            transform.rotation = Quaternion.LookRotation(-offset);
        }

        private void LateUpdate()
        {
            if (_target == null) return;
            transform.position = Vector3.SmoothDamp(transform.position, _target.position + offset, ref _velocity, smoothTime);
        }
    }
}
