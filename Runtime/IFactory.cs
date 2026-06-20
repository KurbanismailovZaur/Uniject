namespace Uniject
{
    public interface IFactory<out TResult>
    {
        public TResult Create();
    }

    public interface IFactory<in TParam, out TResult>
    {
        public TResult Create(TParam origin);
    }
}