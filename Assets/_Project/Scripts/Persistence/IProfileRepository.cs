using System.Threading.Tasks;
using WaveSurvival.Progression;

namespace WaveSurvival.Persistence
{
    /// <summary>
    /// Where the profile lives. Async on purpose: today it is a local file,
    /// tomorrow it can be a web API without changing a single caller.
    /// </summary>
    public interface IProfileRepository
    {
        Task<PlayerProfile> LoadAsync();
        Task SaveAsync(PlayerProfile profile);
    }
}
