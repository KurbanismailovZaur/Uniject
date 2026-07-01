namespace Uniject
{
    public interface IPool<TResult>
    {
        public TResult Spawn();
        public void Despawn(TResult instance);
    }

    public interface IPool<in TParam, TResult>
    {
        public TResult Spawn(TParam origin);
        public void Despawn(TResult instance);
    }
}