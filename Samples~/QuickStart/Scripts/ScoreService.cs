namespace Uniject.Samples.QuickStart
{
    public sealed class ScoreService
    {
        public int Value { get; private set; }

        public void Add(int points) => Value += points;
    }
}
