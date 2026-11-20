using UnityEngine;

namespace WaveSurvival.Core
{
    /// <summary>Immutable description of a single hit.</summary>
    public readonly struct DamageInfo
    {
        public readonly float Amount;
        public readonly Vector3 Point;
        public readonly GameObject Source;

        public DamageInfo(float amount, Vector3 point, GameObject source)
        {
            Amount = amount;
            Point = point;
            Source = source;
        }
    }
}
