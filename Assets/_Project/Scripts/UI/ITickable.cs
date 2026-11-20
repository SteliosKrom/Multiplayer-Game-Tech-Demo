namespace WaveSurvival.UI
{
    /// <summary>Plain C# objects that need a per-frame update get ticked by their installer.</summary>
    public interface ITickable
    {
        void Tick();
    }
}
