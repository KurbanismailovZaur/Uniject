namespace Uniject
{
    public class Provider<T> : IProvider<T>
    {
        public virtual bool HasObject => Object != null;

        public T Object { get; set; }

        public Provider() { }

        public Provider(T obj) => Object = obj;
    }
}