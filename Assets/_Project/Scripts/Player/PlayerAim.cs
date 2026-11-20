using UnityEngine;

namespace WaveSurvival.Player
{
    /// <summary>Rotates the character (yaw only) to face a world point.</summary>
    public class PlayerAim : MonoBehaviour
    {
        [SerializeField] private float turnSpeed = 1080f;

        public void AimAt(Vector3 worldPoint, float deltaTime)
        {
            Vector3 direction = worldPoint - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.01f) return;

            Quaternion target = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, target, turnSpeed * deltaTime);
        }
    }
}
