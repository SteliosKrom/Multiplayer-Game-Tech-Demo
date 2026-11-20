using UnityEngine;

namespace WaveSurvival.Core
{
    /// <summary>Single place that knows the layer names used by the game.</summary>
    public static class GameLayers
    {
        public const string EnvironmentName = "Environment";
        public const string PlayerName = "Player";
        public const string EnemyName = "Enemy";

        public static int Environment => LayerMask.NameToLayer(EnvironmentName);
        public static int Player => LayerMask.NameToLayer(PlayerName);
        public static int Enemy => LayerMask.NameToLayer(EnemyName);

        public static LayerMask PlayerShotMask => LayerMask.GetMask(EnvironmentName, EnemyName);
    }
}
