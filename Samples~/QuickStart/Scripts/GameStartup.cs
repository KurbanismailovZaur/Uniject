using Uniject.Lifecycle;
using UnityEngine;

namespace Uniject.Samples.QuickStart
{
    public sealed class GameStartup : IEntryPoint
    {
        private readonly ScoreService _score;

        // Uniject supplies this constructor dependency.
        public GameStartup(ScoreService score) => _score = score;

        public void Run()
        {
            _score.Add(10);
            Debug.Log(_score.Value); // Prints 10.
        }
    }
}
