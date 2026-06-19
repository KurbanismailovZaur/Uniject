namespace Uniject
{
    public interface IFactory<out TResult>
    {
        public TResult Create();
    }
}