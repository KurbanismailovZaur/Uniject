namespace Uniject
{
    public abstract class CustomFactory
    {
        protected Container _container;

        internal void Construct(Container container) => _container = container;

        internal void InitializeInternal() => Initialize();

        protected virtual void Initialize() { }
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