using UnityEngine;

namespace WaveSurvival.Player
{
    /// <summary>Converts a screen position into a point on the ground plane. Plain C#, no MonoBehaviour.</summary>
    public sealed class AimPointResolver
    {
        private readonly Camera _camera;
        private readonly Plane _ground;

        public AimPointResolver(Camera camera, float groundHeight = 0f)
        {
            _camera = camera;
            _ground = new Plane(Vector3.up, new Vector3(0f, groundHeight, 0f));
        }

        public bool TryGetPoint(Vector2 screenPosition, out Vector3 point)
        {
            Ray ray = _camera.ScreenPointToRay(screenPosition);
            if (_ground.Raycast(ray, out float distance))
            {
                point = ray.GetPoint(distance);
                return true;
            }

            point = default;
            return false;
        }
    }
}
