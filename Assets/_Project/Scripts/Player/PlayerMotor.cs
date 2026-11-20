using UnityEngine;

namespace WaveSurvival.Player
{
    /// <summary>Moves the character on the XZ plane with gravity. Knows nothing about input devices.</summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMotor : MonoBehaviour
    {
        [SerializeField] private float gravity = -20f;

        private CharacterController _controller;
        private float _verticalVelocity;

        private void Awake() => _controller = GetComponent<CharacterController>();

        public void Move(Vector2 input, float speed, float deltaTime)
        {
            Vector3 planar = new Vector3(input.x, 0f, input.y);
            if (planar.sqrMagnitude > 1f) planar.Normalize();

            _verticalVelocity = _controller.isGrounded ? -1f : _verticalVelocity + gravity * deltaTime;

            Vector3 velocity = planar * speed + Vector3.up * _verticalVelocity;
            _controller.Move(velocity * deltaTime);
        }
    }
}
