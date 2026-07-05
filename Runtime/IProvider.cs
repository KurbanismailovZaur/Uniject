namespace Uniject
{
    public interface IProvider<T>
    {
        bool HasData { get; }

        T Data { get; }
    }
}