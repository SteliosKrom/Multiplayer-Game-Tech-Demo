using System.Collections.Generic;
using UnityEngine;
using WaveSurvival.Core;

namespace WaveSurvival.Weapons
{
    /// <summary>
    /// Pure physics: turns "fire weapon X from here towards there" into raycasts and damage.
    /// Knows nothing about input, cooldowns or visuals.
    /// </summary>
    public sealed class HitscanResolver
    {
        private readonly LayerMask _hitMask;
        private readonly List<ShotResult> _results = new();

        public HitscanResolver(LayerMask hitMask) => _hitMask = hitMask;

        public IReadOnlyList<ShotResult> Fire(WeaponDefinition weapon, Vector3 origin, Vector3 forward,
            float damageMultiplier, GameObject shooter)
        {
            _results.Clear();

            for (int i = 0; i < weapon.pellets; i++)
            {
                Vector3 direction = ApplySpread(forward, weapon.spreadAngle);

                if (Physics.Raycast(origin, direction, out RaycastHit hit, weapon.range, _hitMask, QueryTriggerInteraction.Ignore))
                {
                    var target = hit.collider.GetComponentInParent<IDamageable>();
                    if (target != null && target.IsAlive)
                        target.TakeDamage(new DamageInfo(weapon.damage * damageMultiplier, hit.point, shooter));

                    _results.Add(new ShotResult(origin, hit.point, true));
                }
                else
                {
                    _results.Add(new ShotResult(origin, origin + direction * weapon.range, false));
                }
            }

            return _results;
        }

        private static Vector3 ApplySpread(Vector3 forward, float spreadAngle)
        {
            if (spreadAngle <= 0f) return forward;
            float yaw = Random.Range(-spreadAngle, spreadAngle) * 0.5f;
            return Quaternion.Euler(0f, yaw, 0f) * forward;
        }
    }
}
