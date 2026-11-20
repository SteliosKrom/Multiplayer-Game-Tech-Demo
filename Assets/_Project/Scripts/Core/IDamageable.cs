namespace WaveSurvival.Core
{
    /// <summary>Anything that can receive damage (players, enemies).</summary>
    public interface IDamageable
    {
        bool IsAlive { get; }
        void TakeDamage(DamageInfo damage);
    }
}
