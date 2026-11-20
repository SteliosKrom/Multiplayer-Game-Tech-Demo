using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using WaveSurvival.Progression;

namespace WaveSurvival.Persistence
{
    /// <summary>Saves the profile as JSON in Application.persistentDataPath.</summary>
    public sealed class JsonFileProfileRepository : IProfileRepository
    {
        private readonly string _path;

        public JsonFileProfileRepository(string fileName = "profile.json")
        {
            _path = Path.Combine(Application.persistentDataPath, fileName);
        }

        public async Task<PlayerProfile> LoadAsync()
        {
            if (!File.Exists(_path)) return new PlayerProfile();

            try
            {
                string json = await File.ReadAllTextAsync(_path);
                return JsonUtility.FromJson<PlayerProfile>(json) ?? new PlayerProfile();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Save] Could not read profile, starting fresh. {e.Message}");
                return new PlayerProfile();
            }
        }

        public async Task SaveAsync(PlayerProfile profile)
        {
            string json = JsonUtility.ToJson(profile, prettyPrint: true);
            await File.WriteAllTextAsync(_path, json);
        }
    }
}
