namespace Uniject
{
    public interface IFactory<TResult>
    {
        public TResult Create();
    }
}