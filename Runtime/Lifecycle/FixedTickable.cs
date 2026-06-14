namespace Uniject.Lifecycle
{
    internal sealed class FixedTickable : TickableBase<IFixedTickable>
    {
        public override void Tick()
        {
            _isTicking = true;

            try
            {
                foreach (var tickable in _tickables)
                {
                    if (!_tickablesPendingRemove.Contains(tickable))
                        tickable.FixedTick();
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
