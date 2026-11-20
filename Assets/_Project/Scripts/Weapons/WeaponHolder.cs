using System;
using UnityEngine;

namespace WaveSurvival.Weapons
{
    /// <summary>
    /// Holds the equipped weapon and its fire cooldown. Decides WHEN a shot may happen,
    /// delegates WHAT a shot does to HitscanResolver and HOW it looks to TracerPool.
    /// </summary>
    public class WeaponHolder : MonoBehaviour
    {
        [SerializeField] private Transform muzzle;

        private HitscanResolver _resolver;
        private TracerPool _tracers;
        private float _nextShotTime;

        public WeaponDefinition Current { get; private set; }
        public Transform Muzzle => muzzle;
        public event Action<WeaponDefinition> Equipped;

        public void Construct(HitscanResolver resolver, TracerPool tracers)
        {
            _resolver = resolver;
            _tracers = tracers;
        }

        public void Equip(WeaponDefinition weapon)
        {
            if (weapon == null || weapon == Current) return;
            Current = weapon;
            _nextShotTime = Time.time + 0.15f;
            Equipped?.Invoke(weapon);
        }

        public bool CanFire => Current != null && Time.time >= _nextShotTime;

        public void TryFire(float damageMultiplier)
        {
            if (!CanFire) return;
            _nextShotTime = Time.time + Current.SecondsBetweenShots;

            var shots = _resolver.Fire(Current, muzzle.position, muzzle.forward, damageMultiplier, gameObject);
            foreach (var shot in shots) _tracers.Show(shot.Origin, shot.End, Current.tracerColor);
        }
    }
}
