using System;
using System.Threading.Tasks;
using UnityEngine;
using WaveSurvival.Progression;

namespace WaveSurvival.Persistence
{
    /// <summary>Holds the in-memory profile for this scene and knows how to persist it.</summary>
    public sealed class ProfileSession
    {
        private readonly IProfileRepository _repository;

        public PlayerProfile Profile { get; private set; }

        public ProfileSession(IProfileRepository repository) => _repository = repository;

        public async Task LoadAsync() => Profile = await _repository.LoadAsync();

        public async Task SaveAsync()
        {
            if (Profile == null) return;
            try
            {
                await _repository.SaveAsync(Profile);
            }
            catch (Exception e)
            {
                Debug.LogError($"[Save] Failed to save profile: {e.Message}");
            }
        }

        /// <summary>Fire-and-forget save for button handlers.</summary>
        public void SaveInBackground() => _ = SaveAsync();
    }
}
