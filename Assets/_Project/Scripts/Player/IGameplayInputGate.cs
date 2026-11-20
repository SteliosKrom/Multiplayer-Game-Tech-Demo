namespace WaveSurvival.Player
{
    /// <summary>Lets UI (shop, pause, game over) block gameplay input without the player knowing about UI.</summary>
    public interface IGameplayInputGate
    {
        bool IsMovementBlocked { get; }
        bool IsFiringBlocked { get; }
    }
}
