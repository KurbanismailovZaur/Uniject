namespace Uniject.Lifecycle
{
    internal sealed class Tickable : TickableBase<ITickable>
    {
        public override void Tick()
        {
            _isTicking = true;

            try
            {
                foreach (var tickable in _tickables)
                {
                    if (!_tickablesPendingRemove.Contains(tickable))
                        tickable.Tick();
                }
            }
            finally
            {
                _isTicking = false;

                RemoveQueuedTickables();
                AddQueuedTickables();
            }
        }
    }
}
