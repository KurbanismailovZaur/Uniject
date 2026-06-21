namespace Uniject
{
    public abstract class CustomFactory
    {
        protected IObjectBuilder _objectBuilder;

        internal void Construct(IObjectBuilder objectBuilder) => _objectBuilder = objectBuilder;
    }
    
    public abstract class CustomFactory<TResult> : CustomFactory, IFactory<TResult>
    {
        public abstract TResult Create();
    }

    public abstract class CustomFactory<TParam,TResult> : CustomFactory, IFactory<TParam, TResult>
    {
        public abstract TResult Create(TParam origin);
    }
}