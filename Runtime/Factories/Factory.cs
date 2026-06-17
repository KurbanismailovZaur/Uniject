namespace Uniject.Factories
{
    public abstract class Factory<TResult>
    {
        protected IObjectBuilder _objectBuilder;

        public Factory(IObjectBuilder objectBuilder) => _objectBuilder = objectBuilder;

        public abstract TResult Create();
    }
}