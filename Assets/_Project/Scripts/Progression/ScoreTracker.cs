using System;

namespace WaveSurvival.Progression
{
    /// <summary>Score of the current run only. Reset every time a run starts.</summary>
    public sealed class ScoreTracker
    {
        public int Score { get; private set; }
        public int Kills { get; private set; }

        public event Action<int> ScoreChanged;

        public void AddKill(int points)
        {
            Kills++;
            Add(points);
        }

        public void Add(int points)
        {
            if (points <= 0) return;
            Score += points;
            ScoreChanged?.Invoke(Score);
        }
    }
}
