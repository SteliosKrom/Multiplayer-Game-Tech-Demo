using UnityEngine;

namespace WaveSurvival.Weapons
{
    /// <summary>Designer data for one weapon. No behaviour lives here.</summary>
    [CreateAssetMenu(menuName = "Wave Survival/Weapon Definition")]
    public class WeaponDefinition : ScriptableObject
    {
        [Tooltip("Stable id used in save files and the backend. Never rename after release.")]
        public string id = "pistol";
        public string displayName = "Pistol";
        [Min(0)] public int price;
        public bool ownedByDefault;

        [Header("Ballistics")]
        [Min(0.1f)] public float damage = 20f;
        [Tooltip("Shots per second")] [Min(0.1f)] public float fireRate = 3f;
        [Min(1)] public int pellets = 1;
        [Tooltip("Total cone in degrees")] [Min(0f)] public float spreadAngle = 2f;
        [Min(1f)] public float range = 40f;

        [Header("Visuals")]
        public Color tracerColor = new Color(1f, 0.85f, 0.3f);

        public float SecondsBetweenShots => 1f / fireRate;
    }
}
