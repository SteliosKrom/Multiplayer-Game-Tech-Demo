using UnityEngine;
using WaveSurvival.Combat;
using WaveSurvival.Input;
using WaveSurvival.Progression;
using WaveSurvival.Weapons;

namespace WaveSurvival.Player
{
    /// <summary>
    /// The player's "brain". Reads IPlayerInput each frame and forwards it to the
    /// motor, aim and weapon components. Every dependency arrives through Construct.
    /// </summary>
    [RequireComponent(typeof(PlayerMotor), typeof(PlayerAim), typeof(WeaponHolder))]
    [RequireComponent(typeof(Health))]
    public class PlayerController : MonoBehaviour
    {
        private PlayerMotor _motor;
        private PlayerAim _aim;
        private WeaponHolder _weapons;
        private Health _health;

        private IPlayerInput _input;
        private AimPointResolver _aimResolver;
        private PlayerLoadout _loadout;
        private IGameplayInputGate _gate;
        private PlayerStats _stats;

        public Health Health => _health;
        public WeaponHolder Weapons => _weapons;

        private void Awake()
        {
            _motor = GetComponent<PlayerMotor>();
            _aim = GetComponent<PlayerAim>();
            _weapons = GetComponent<WeaponHolder>();
            _health = GetComponent<Health>();
        }

        public void Construct(IPlayerInput input, AimPointResolver aimResolver, PlayerLoadout loadout,
            IGameplayInputGate gate, HitscanResolver hitscan, TracerPool tracers)
        {
            _input = input;
            _aimResolver = aimResolver;
            _loadout = loadout;
            _gate = gate;
            _weapons.Construct(hitscan, tracers);
            _weapons.Equip(loadout.BestOwned());
        }

        /// <summary>Called on spawn and on every level up.</summary>
        public void ApplyStats(PlayerStats stats, bool refillHealth)
        {
            _stats = stats;
            _health.SetMax(stats.MaxHealth, refillHealth);
        }

        private void Update()
        {
            if (_input == null || !_health.IsAlive) return;
            float dt = Time.deltaTime;

            if (!_gate.IsMovementBlocked)
                _motor.Move(_input.Move, _stats.MoveSpeed, dt);

            if (_aimResolver.TryGetPoint(_input.PointerScreenPosition, out Vector3 aimPoint))
                _aim.AimAt(aimPoint, dt);

            int slot = _input.WeaponSlotPressed;
            if (slot >= 0) _weapons.Equip(_loadout.GetSlot(slot));

            if (_input.FireHeld && !_gate.IsFiringBlocked)
                _weapons.TryFire(_stats.DamageMultiplier);
        }
    }
}
