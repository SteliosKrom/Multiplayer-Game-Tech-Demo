using UnityEngine.EventSystems;
using WaveSurvival.Player;

namespace WaveSurvival.UI
{
    /// <summary>Which overlays are open. Implements the input gate so the player is blocked by UI without knowing UI exists.</summary>
    public sealed class UiState : IGameplayInputGate
    {
        public bool ShopOpen { get; set; }
        public bool Paused { get; set; }
        public bool GameOver { get; set; }

        public bool IsMovementBlocked => Paused || GameOver;
        public bool IsFiringBlocked => ShopOpen || Paused || GameOver || IsPointerOverUi();

        private static bool IsPointerOverUi() =>
            EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }
}
