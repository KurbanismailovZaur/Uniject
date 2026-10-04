namespace Uniject
{
    public interface IProvider<T>
    {
        bool HasObject { get; }

        T Object { get; }
    }
}