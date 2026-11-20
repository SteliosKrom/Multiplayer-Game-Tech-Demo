using System;
using System.Collections.Generic;

namespace WaveSurvival.Progression
{
    /// <summary>
    /// Plain serializable data that survives between runs. It is the "save file".
    /// Field names are lowerCamelCase so the same JSON shape can later be sent to a web API.
    /// </summary>
    [Serializable]
    public class PlayerProfile
    {
        public int level = 1;
        public int xp;
        public int gold;
        public List<string> ownedWeapons = new();
        public int highScore;
        public int bestWave;
    }
}
