using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace WaveSurvival.Input
{
    /// <summary>IPlayerInput backed by the Input System. Actions are created in code so no asset is needed.</summary>
    public sealed class InputSystemPlayerInput : IPlayerInput, IDisposable
    {
        private const int SlotCount = 4;

        private readonly InputAction _move;
        private readonly InputAction _point;
        private readonly InputAction _fire;
        private readonly InputAction _shop;
        private readonly InputAction _pause;
        private readonly InputAction[] _slots = new InputAction[SlotCount];

        public InputSystemPlayerInput()
        {
            _move = new InputAction("Move", InputActionType.Value);
            _move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
            _move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/upArrow").With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow").With("Right", "<Keyboard>/rightArrow");

            _point = new InputAction("Point", InputActionType.Value, "<Pointer>/position");
            _fire = new InputAction("Fire", InputActionType.Button, "<Mouse>/leftButton");
            _shop = new InputAction("Shop", InputActionType.Button, "<Keyboard>/b");
            _pause = new InputAction("Pause", InputActionType.Button, "<Keyboard>/escape");

            for (int i = 0; i < SlotCount; i++)
                _slots[i] = new InputAction("Slot" + (i + 1), InputActionType.Button, "<Keyboard>/" + (i + 1));

            ForEach(a => a.Enable());
        }

        public Vector2 Move => _move.ReadValue<Vector2>();
        public Vector2 PointerScreenPosition => _point.ReadValue<Vector2>();
        public bool FireHeld => _fire.IsPressed();
        public bool ShopPressed => _shop.WasPressedThisFrame();
        public bool PausePressed => _pause.WasPressedThisFrame();

        public int WeaponSlotPressed
        {
            get
            {
                for (int i = 0; i < SlotCount; i++)
                    if (_slots[i].WasPressedThisFrame()) return i;
                return -1;
            }
        }

        public void Dispose() => ForEach(a => a.Dispose());

        private void ForEach(Action<InputAction> action)
        {
            action(_move); action(_point); action(_fire); action(_shop); action(_pause);
            foreach (var slot in _slots) action(slot);
        }
    }
}
