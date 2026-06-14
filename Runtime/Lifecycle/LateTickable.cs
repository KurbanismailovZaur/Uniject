namespace Uniject.Lifecycle
{
    internal sealed class LateTickable : TickableBase<ILateTickable>
    {
        public override void Tick()
        {
            _isTicking = true;

            try
            {
                foreach (var tickable in _tickables)
                {
                    if (!_tickablesPendingRemove.Contains(tickable))
                        tickable.LateTick();
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
