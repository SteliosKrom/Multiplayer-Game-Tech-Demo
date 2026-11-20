using UnityEngine;

namespace WaveSurvival.Input
{
    /// <summary>
    /// What the game needs from "a player's hands". Gameplay code depends on this,
    /// never on the Input System directly, so it can be replaced by AI, replays or tests.
    /// </summary>
    public interface IPlayerInput
    {
        Vector2 Move { get; }
        Vector2 PointerScreenPosition { get; }
        bool FireHeld { get; }
        /// <summary>0-based weapon slot pressed this frame, or -1.</summary>
        int WeaponSlotPressed { get; }
        bool ShopPressed { get; }
        bool PausePressed { get; }
    }
}
