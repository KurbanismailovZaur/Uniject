using System;
using System.Collections.Generic;
using NUnit.Framework;
using Uniject.Attributes;
using Uniject.Bindings;
using Uniject.Bindings.Pools;
using Uniject.InstanceGetters;

namespace Uniject.Tests
{
    public class ContainerInjectContextTests
    {
        private interface IService { }

        private class Service : IService { }

        private class ConstructorConsumer
        {
            public IService Service { get; }

            public ConstructorConsumer(IService service)
            {
                Service = service;
            }
        }

        private class InnerConsumer
        {
            public IService Service { get; }

            public InnerConsumer(IService service)
            {
                Service = service;
            }
        }

        private class OuterConsumer
        {
            public InnerConsumer Inner { get; }

            public OuterConsumer(InnerConsumer inner)
            {
                Inner = inner;
            }
        }

        private class MultiParameterConstructorConsumer
        {
            public IService First { get; }
            public IService Second { get; }

            public MultiParameterConstructorConsumer(IService first, IService second)
            {
                First = first;
                Second = second;
            }
        }

        private class MultiParameterMethodConsumer
        {
            public IService First { get; private set; }
            public IService Second { get; private set; }

            [Inject]
            public void Construct(IService first, IService second)
            {
                First = first;
                Second = second;
            }
        }

        private class MethodConsumer
        {
            public IService Service { get; private set; }

            [Inject]
            public void Construct(IService service)
            {
                Service = service;
            }
        }

        private class BaseMethodConsumer
        {
            public IService Service { get; private set; }

            [Inject]
            protected void Construct(IService service)
            {
                Service = service;
            }
        }

        private class DerivedMethodConsumer : BaseMethodConsumer { }

        private class ServiceFactory : Factory<IService>
        {
            public ServiceFactory() { }
        }

        private class ParameterizedServiceFactory : Factory<string, IService>
        {
            public ParameterizedServiceFactory() { }
        }

        private class ServicePool : Pool<IService>
        {
            public ServicePool() { }
        }

        private class ForwardingGetter : InstanceGetter
        {
            public ForwardingGetter(Container container) : base(container) { }

            public override object GetInstance(Type concreteType, CreateOptions createOptions,
                InjectContext context)
            {
                return ResolveWithContext(concreteType, context);
            }
        }

        private static void AssertRootContext(InjectContext context, Type contractType)
        {
            Assert.That(context.ContractType, Is.EqualTo(contractType));
            Assert.That(context.ConsumerType, Is.Null);
            Assert.That(context.ConsumerInstance, Is.Null);
            Assert.That(context.ParameterInfo, Is.Null);
        }

        private static void AssertRegularFromMethodWithoutInspectingContext(
            Action<Container, Func<InjectContext, Service>> configure)
        {
            var container = new Container();
            configure(container, _ => new Service());

            Assert.That(container.Resolve<IService>(), Is.TypeOf<Service>());
        }

        private static void AssertContextualRegularFromMethod(
            Action<Container, Func<InjectContext, Service>> configure)
        {
            var receivedContext = default(InjectContext);
            var container = new Container();
            configure(container, context =>
            {
                receivedContext = context;
                return new Service();
            });

            Assert.That(container.Resolve<IService>(), Is.TypeOf<Service>());
            AssertRootContext(receivedContext, typeof(IService));
        }

        private static void AssertFactoryFromMethodWithoutInspectingContext(
            Action<Container, Func<InjectContext, Service>> configure)
        {
            var container = new Container();
            configure(container, _ => new Service());

            Assert.That(container.Resolve<ServiceFactory>().Create(), Is.TypeOf<Service>());
        }

        private static void AssertContextualFactoryFromMethod(
            Action<Container, Func<InjectContext, Service>> configure)
        {
            var receivedContext = default(InjectContext);
            var container = new Container();
            configure(container, context =>
            {
                receivedContext = context;
                return new Service();
            });

            Assert.That(container.Resolve<ServiceFactory>().Create(), Is.TypeOf<Service>());
            AssertRootContext(receivedContext, typeof(IService));
        }

        private static void AssertParameterizedFactoryFromMethodWithoutInspectingContext(
            Action<Container, Func<string, InjectContext, Service>> configure)
        {
            var container = new Container();
            configure(container, (_, __) => new Service());

            Assert.That(
                container.Resolve<ParameterizedServiceFactory>().Create("origin"),
                Is.TypeOf<Service>());
        }

        private static void AssertContextualParameterizedFactoryFromMethod(
            Action<Container, Func<string, InjectContext, Service>> configure)
        {
            var receivedContext = default(InjectContext);
            var container = new Container();
            configure(container, (_, context) =>
            {
                receivedContext = context;
                return new Service();
            });

            Assert.That(
                container.Resolve<ParameterizedServiceFactory>().Create("origin"),
                Is.TypeOf<Service>());
            AssertRootContext(receivedContext, typeof(IService));
        }

        private static void AssertPoolFromMethodWithoutInspectingContext(
            Action<Container, Func<InjectContext, Service>> configure)
        {
            var container = new Container();
            configure(container, _ => new Service());

            Assert.That(container.Resolve<ServicePool>().Spawn(), Is.TypeOf<Service>());
        }

        private static void AssertContextualPoolFromMethod(
            Action<Container, Func<InjectContext, Service>> configure)
        {
            var receivedContext = default(InjectContext);
            var container = new Container();
            configure(container, context =>
            {
                receivedContext = context;
                return new Service();
            });

            Assert.That(container.Resolve<ServicePool>().Spawn(), Is.TypeOf<Service>());
            AssertRootContext(receivedContext, typeof(IService));
        }

        [Test]
        public void Resolve_FromContextualMethod_ProvidesRootContextWithContractType()
        {
            var expected = new Service();
            var receivedContext = default(InjectContext);
            var container = new Container();
            container.Bind<IService>().To<Service>().FromMethod(context =>
            {
                receivedContext = context;
                return expected;
            }).AsTransient();

            var resolved = container.Resolve<IService>();

            Assert.That(resolved, Is.SameAs(expected));
            AssertRootContext(receivedContext, typeof(IService));
        }

        [Test]
        public void TryResolve_FromContextualMethod_ProvidesRootContextWithContractType()
        {
            var expected = new Service();
            var receivedContext = default(InjectContext);
            var container = new Container();
            container.Bind<IService>().To<Service>().FromMethod(context =>
            {
                receivedContext = context;
                return expected;
            }).AsTransient();

            var (resolved, wasResolved) = container.TryResolve<IService>();

            Assert.That(wasResolved, Is.True);
            Assert.That(resolved, Is.SameAs(expected));
            AssertRootContext(receivedContext, typeof(IService));
        }

        [Test]
        public void BindingGetInstance_ProvidesRootContextWithBindingContractType()
        {
            var expected = new Service();
            var receivedContext = default(InjectContext);
            var container = new Container();
            var binding = new BindingToType(container, typeof(IService))
            {
                ConcreteType = typeof(Service),
                InstanceGetter = new InstanceGetterFromMethod<Service>(container, context =>
                {
                    receivedContext = context;
                    return expected;
                })
            };

            var resolved = binding.GetInstance();

            Assert.That(resolved, Is.SameAs(expected));
            AssertRootContext(receivedContext, typeof(IService));
        }

        [Test]
        public void Instantiate_ConstructorParameter_ProvidesConsumerTypeAndParameterInfo()
        {
            var expected = new Service();
            var receivedContext = default(InjectContext);
            var container = new Container();
            container.Bind<IService>().FromMethod(context =>
            {
                receivedContext = context;
                return expected;
            }).AsTransient();

            var consumer = container.Instantiate<ConstructorConsumer>();

            Assert.That(consumer.Service, Is.SameAs(expected));
            Assert.That(receivedContext.ContractType, Is.EqualTo(typeof(IService)));
            Assert.That(receivedContext.ConsumerType, Is.EqualTo(typeof(ConstructorConsumer)));
            Assert.That(receivedContext.ConsumerInstance, Is.Null);
            Assert.That(receivedContext.ParameterInfo, Is.Not.Null);
            Assert.That(receivedContext.ParameterInfo.ParameterType, Is.EqualTo(typeof(IService)));
            Assert.That(receivedContext.ParameterInfo.Position, Is.EqualTo(0));
            Assert.That(receivedContext.ParameterInfo.Member.DeclaringType,
                Is.EqualTo(typeof(ConstructorConsumer)));
        }

        [Test]
        public void Instantiate_MultipleConstructorParameters_ProvidesContextForEachParameter()
        {
            var receivedContexts = new List<InjectContext>();
            var container = new Container();
            container.Bind<IService>().FromMethod(context =>
            {
                receivedContexts.Add(context);
                return new Service();
            }).AsTransient();

            var consumer = container.Instantiate<MultiParameterConstructorConsumer>();

            Assert.That(consumer.First, Is.Not.SameAs(consumer.Second));
            Assert.That(receivedContexts, Has.Count.EqualTo(2));
            Assert.That(receivedContexts[0].ConsumerType,
                Is.EqualTo(typeof(MultiParameterConstructorConsumer)));
            Assert.That(receivedContexts[0].ConsumerInstance, Is.Null);
            Assert.That(receivedContexts[0].ParameterInfo.Position, Is.EqualTo(0));
            Assert.That(receivedContexts[1].ConsumerType,
                Is.EqualTo(typeof(MultiParameterConstructorConsumer)));
            Assert.That(receivedContexts[1].ConsumerInstance, Is.Null);
            Assert.That(receivedContexts[1].ParameterInfo.Position, Is.EqualTo(1));
        }

        [Test]
        public void Inject_MethodParameter_ProvidesRuntimeConsumerAndParameterInfo()
        {
            var expected = new Service();
            var receivedContext = default(InjectContext);
            var consumer = new DerivedMethodConsumer();
            var container = new Container();
            container.Bind<IService>().FromMethod(context =>
            {
                receivedContext = context;
                return expected;
            }).AsTransient();

            container.Inject(consumer);

            Assert.That(consumer.Service, Is.SameAs(expected));
            Assert.That(receivedContext.ContractType, Is.EqualTo(typeof(IService)));
            Assert.That(receivedContext.ConsumerType, Is.EqualTo(typeof(DerivedMethodConsumer)));
            Assert.That(receivedContext.ConsumerInstance, Is.SameAs(consumer));
            Assert.That(receivedContext.ParameterInfo, Is.Not.Null);
            Assert.That(receivedContext.ParameterInfo.ParameterType, Is.EqualTo(typeof(IService)));
            Assert.That(receivedContext.ParameterInfo.Member.DeclaringType,
                Is.EqualTo(typeof(BaseMethodConsumer)));
        }

        [Test]
        public void Inject_MultipleMethodParameters_ProvidesContextForEachParameter()
        {
            var receivedContexts = new List<InjectContext>();
            var consumer = new MultiParameterMethodConsumer();
            var container = new Container();
            container.Bind<IService>().FromMethod(context =>
            {
                receivedContexts.Add(context);
                return new Service();
            }).AsTransient();

            container.Inject(consumer);

            Assert.That(consumer.First, Is.Not.SameAs(consumer.Second));
            Assert.That(receivedContexts, Has.Count.EqualTo(2));
            Assert.That(receivedContexts[0].ConsumerType,
                Is.EqualTo(typeof(MultiParameterMethodConsumer)));
            Assert.That(receivedContexts[0].ConsumerInstance, Is.SameAs(consumer));
            Assert.That(receivedContexts[0].ParameterInfo.Position, Is.EqualTo(0));
            Assert.That(receivedContexts[1].ConsumerType,
                Is.EqualTo(typeof(MultiParameterMethodConsumer)));
            Assert.That(receivedContexts[1].ConsumerInstance, Is.SameAs(consumer));
            Assert.That(receivedContexts[1].ParameterInfo.Position, Is.EqualTo(1));
        }

        [Test]
        public void Instantiate_NestedDependency_ProvidesOnlyImmediateConsumer()
        {
            var expected = new Service();
            var receivedContext = default(InjectContext);
            var container = new Container();
            container.Bind<IService>().FromMethod(context =>
            {
                receivedContext = context;
                return expected;
            }).AsTransient();
            container.Bind<InnerConsumer>().AsTransient();

            var outer = container.Instantiate<OuterConsumer>();

            Assert.That(outer.Inner.Service, Is.SameAs(expected));
            Assert.That(receivedContext.ConsumerType, Is.EqualTo(typeof(InnerConsumer)));
        }

        [Test]
        public void Inject_WhenBindingComesFromParent_PreservesChildConsumerContext()
        {
            var expected = new Service();
            var receivedContext = default(InjectContext);
            var parent = new Container();
            parent.Bind<IService>().FromMethod(context =>
            {
                receivedContext = context;
                return expected;
            }).AsTransient();
            var child = new Container(parent);
            var consumer = new MethodConsumer();

            child.Inject(consumer);

            Assert.That(consumer.Service, Is.SameAs(expected));
            Assert.That(receivedContext.ConsumerInstance, Is.SameAs(consumer));
            Assert.That(receivedContext.ConsumerType, Is.EqualTo(typeof(MethodConsumer)));
        }

        [Test]
        public void Inject_FromResolve_ForwardsOriginalContextWithoutChangingContract()
        {
            var expected = new Service();
            var receivedContext = default(InjectContext);
            var container = new Container();
            container.Bind<Service>().FromMethod(context =>
            {
                receivedContext = context;
                return expected;
            }).AsTransient();
            container.Bind<IService>().To<Service>().FromResolve().AsTransient();
            var consumer = new MethodConsumer();

            container.Inject(consumer);

            Assert.That(consumer.Service, Is.SameAs(expected));
            Assert.That(receivedContext.ContractType, Is.EqualTo(typeof(IService)));
            Assert.That(receivedContext.ConsumerInstance, Is.SameAs(consumer));
            Assert.That(receivedContext.ConsumerType, Is.EqualTo(typeof(MethodConsumer)));
        }

        [Test]
        public void Inject_FromResolveGetter_ForwardsOriginalContextToSourceResolve()
        {
            var expected = new Service();
            var receivedContext = default(InjectContext);
            var container = new Container();
            container.Bind<Service>().FromMethod(context =>
            {
                receivedContext = context;
                return expected;
            }).AsTransient();
            container.Bind<IService>().FromResolveGetter<Service>(service => service).AsTransient();
            var consumer = new MethodConsumer();

            container.Inject(consumer);

            Assert.That(consumer.Service, Is.SameAs(expected));
            Assert.That(receivedContext.ContractType, Is.EqualTo(typeof(IService)));
            Assert.That(receivedContext.ConsumerType, Is.EqualTo(typeof(MethodConsumer)));
            Assert.That(receivedContext.ConsumerInstance, Is.SameAs(consumer));
            Assert.That(receivedContext.ParameterInfo, Is.Not.Null);
            Assert.That(receivedContext.ParameterInfo.ParameterType, Is.EqualTo(typeof(IService)));
            Assert.That(receivedContext.ParameterInfo.Position, Is.EqualTo(0));
            Assert.That(receivedContext.ParameterInfo.Member.DeclaringType,
                Is.EqualTo(typeof(MethodConsumer)));
        }

        [Test]
        public void Inject_FromCachedSubcontainer_ForwardsEachOriginalContext()
        {
            var expected = new Service();
            var receivedContexts = new List<InjectContext>();
            var subcontainer = new Container();
            subcontainer.Bind<Service>().FromMethod(context =>
            {
                receivedContexts.Add(context);
                return expected;
            }).AsTransient();

            var container = new Container();
            container.Bind<IService>().To<Service>()
                .FromSubcontainerResolve()
                .ByInstance(subcontainer)
                .AsCached();
            var first = new MethodConsumer();
            var second = new MethodConsumer();

            container.Inject(first);
            container.Inject(second);

            Assert.That(receivedContexts, Has.Count.EqualTo(2));
            Assert.That(receivedContexts[0].ContractType, Is.EqualTo(typeof(IService)));
            Assert.That(receivedContexts[0].ConsumerInstance, Is.SameAs(first));
            Assert.That(receivedContexts[1].ContractType, Is.EqualTo(typeof(IService)));
            Assert.That(receivedContexts[1].ConsumerInstance, Is.SameAs(second));
        }

        [Test]
        public void Inject_FromTransientSubcontainer_ForwardsEachOriginalContext()
        {
            var receivedContexts = new List<InjectContext>();
            var container = new Container();
            container.Bind<IService>().To<Service>()
                .FromSubcontainerResolve()
                .ByMethod(subcontainer =>
                {
                    subcontainer.Bind<Service>().FromMethod(context =>
                    {
                        receivedContexts.Add(context);
                        return new Service();
                    }).AsTransient();
                })
                .AsTransient();
            var first = new MethodConsumer();
            var second = new MethodConsumer();

            container.Inject(first);
            container.Inject(second);

            Assert.That(receivedContexts, Has.Count.EqualTo(2));
            Assert.That(receivedContexts[0].ContractType, Is.EqualTo(typeof(IService)));
            Assert.That(receivedContexts[0].ConsumerInstance, Is.SameAs(first));
            Assert.That(receivedContexts[1].ContractType, Is.EqualTo(typeof(IService)));
            Assert.That(receivedContexts[1].ConsumerInstance, Is.SameAs(second));
        }

        [Test]
        public void Inject_AsCached_UsesContextOfFirstSuccessfulMaterialization()
        {
            var expected = new Service();
            var receivedContexts = new List<InjectContext>();
            var callsCount = 0;
            var container = new Container();
            container.Bind<IService>().FromMethod(context =>
            {
                callsCount++;

                if (callsCount == 1)
                    throw new InvalidOperationException("First creation failed.");

                receivedContexts.Add(context);
                return expected;
            }).AsCached();
            var failedConsumer = new MethodConsumer();
            var successfulConsumer = new MethodConsumer();
            var laterConsumer = new MethodConsumer();

            Assert.That(
                () => container.Inject(failedConsumer),
                Throws.TypeOf<InvalidOperationException>());

            container.Inject(successfulConsumer);
            container.Inject(laterConsumer);

            Assert.That(callsCount, Is.EqualTo(2));
            Assert.That(receivedContexts, Has.Count.EqualTo(1));
            Assert.That(receivedContexts[0].ConsumerInstance, Is.SameAs(successfulConsumer));
            Assert.That(successfulConsumer.Service, Is.SameAs(expected));
            Assert.That(laterConsumer.Service, Is.SameAs(expected));
        }

        [Test]
        public void Build_NonLazyCachedBinding_UsesRootContext()
        {
            var expected = new Service();
            var receivedContexts = new List<InjectContext>();
            var container = new Container();
            container.Bind<IService>().FromMethod(context =>
            {
                receivedContexts.Add(context);
                return expected;
            }).AsCached().NonLazy();

            container.Build();
            var consumer = new MethodConsumer();
            container.Inject(consumer);

            Assert.That(receivedContexts, Has.Count.EqualTo(1));
            AssertRootContext(receivedContexts[0], typeof(IService));
            Assert.That(consumer.Service, Is.SameAs(expected));
        }

        [Test]
        public void Build_NonLazyTransientBinding_FirstResolveUsesPrewarmedRootThenConsumerContext()
        {
            var receivedContexts = new List<InjectContext>();
            var container = new Container();
            container.Bind<IService>().FromMethod(context =>
            {
                receivedContexts.Add(context);
                return new Service();
            }).AsTransient().NonLazy();

            container.Build();
            var first = new MethodConsumer();
            var second = new MethodConsumer();
            container.Inject(first);
            container.Inject(second);

            Assert.That(receivedContexts, Has.Count.EqualTo(2));
            AssertRootContext(receivedContexts[0], typeof(IService));
            Assert.That(receivedContexts[1].ConsumerInstance, Is.SameAs(second));
        }

        [Test]
        public void FactoryCreate_UsesResultContractRootContext()
        {
            var receivedContext = default(InjectContext);
            var container = new Container();
            container.BindFactory<IService, ServiceFactory>()
                .To<Service>()
                .FromMethod(context =>
                {
                    receivedContext = context;
                    return new Service();
                })
                .AsCached();
            var factory = container.Resolve<ServiceFactory>();

            var service = factory.Create();

            Assert.That(service, Is.TypeOf<Service>());
            AssertRootContext(receivedContext, typeof(IService));
        }

        [Test]
        public void ParameterizedFactoryCreate_KeepsOriginSeparateAndUsesResultContractRoot()
        {
            const string expectedOrigin = "origin";
            var receivedOrigin = default(string);
            var receivedContext = default(InjectContext);
            var container = new Container();
            container.BindFactory<string, IService, ParameterizedServiceFactory>()
                .To<Service>()
                .FromMethod((origin, context) =>
                {
                    receivedOrigin = origin;
                    receivedContext = context;
                    return new Service();
                })
                .AsCached();
            var factory = container.Resolve<ParameterizedServiceFactory>();

            var service = factory.Create(expectedOrigin);

            Assert.That(service, Is.TypeOf<Service>());
            Assert.That(receivedOrigin, Is.EqualTo(expectedOrigin));
            AssertRootContext(receivedContext, typeof(IService));
        }

        [Test]
        public void PoolPrewarmAndExpansion_UseResultContractRootContexts()
        {
            var receivedContexts = new List<InjectContext>();
            var container = new Container();
            container.BindPool<IService, ServicePool>()
                .WithInitialSize(1)
                .WithMaxSize(2)
                .ExpandByOne()
                .To<Service>()
                .FromMethod(context =>
                {
                    receivedContexts.Add(context);
                    return new Service();
                })
                .AsCached();
            var pool = container.Resolve<ServicePool>();

            var first = pool.Spawn();
            var second = pool.Spawn();

            Assert.That(first, Is.TypeOf<Service>());
            Assert.That(second, Is.TypeOf<Service>());
            Assert.That(receivedContexts, Has.Count.EqualTo(2));
            AssertRootContext(receivedContexts[0], typeof(IService));
            AssertRootContext(receivedContexts[1], typeof(IService));
        }

        [Test]
        public void PublicPoolInitialize_UsesGenericResultAsRootContract()
        {
            var receivedContext = default(InjectContext);
            var container = new Container();
            var getter = new InstanceGetterFromMethod<Service>(container, context =>
            {
                receivedContext = context;
                return new Service();
            });
            var pool = new ServicePool();
            pool.Initialize(
                getter,
                typeof(Service),
                1,
                1,
                ExpandType.ByOne,
                false);

            var service = pool.Spawn();

            Assert.That(service, Is.TypeOf<Service>());
            AssertRootContext(receivedContext, typeof(IService));
        }

        [Test]
        public void FromMethod_CanIgnoreContextOnEveryFluentSurface()
        {
            AssertRegularFromMethodWithoutInspectingContext((container, method) =>
                container.Bind<IService>().To<Service>().FromMethod(method).AsTransient());
            AssertRegularFromMethodWithoutInspectingContext((container, method) =>
                container.Bind<IService>().FromMethod(method).AsTransient());
            AssertRegularFromMethodWithoutInspectingContext((container, method) =>
                container.Bind(typeof(IService)).FromMethod<Service>(method).AsTransient());

            AssertFactoryFromMethodWithoutInspectingContext((container, method) =>
                container.BindFactory<IService, ServiceFactory>().FromMethod(method).AsCached());
            AssertFactoryFromMethodWithoutInspectingContext((container, method) =>
                container.BindFactory<IService, ServiceFactory>()
                    .To<Service>()
                    .FromMethod(method)
                    .AsCached());

            AssertParameterizedFactoryFromMethodWithoutInspectingContext((container, method) =>
                container.BindFactory<string, IService, ParameterizedServiceFactory>()
                    .FromMethod(method)
                    .AsCached());
            AssertParameterizedFactoryFromMethodWithoutInspectingContext((container, method) =>
                container.BindFactory<string, IService, ParameterizedServiceFactory>()
                    .To<Service>()
                    .FromMethod(method)
                    .AsCached());

            AssertPoolFromMethodWithoutInspectingContext((container, method) =>
                container.BindPool<IService, ServicePool>().FromMethod(method).AsCached());
            AssertPoolFromMethodWithoutInspectingContext((container, method) =>
                container.BindPool<IService, ServicePool>()
                    .WithInitialSize(0)
                    .FromMethod(method)
                    .AsCached());
            AssertPoolFromMethodWithoutInspectingContext((container, method) =>
                container.BindPool<IService, ServicePool>()
                    .WithMaxSize(2)
                    .FromMethod(method)
                    .AsCached());
            AssertPoolFromMethodWithoutInspectingContext((container, method) =>
                container.BindPool<IService, ServicePool>()
                    .ExpandByOne()
                    .FromMethod(method)
                    .AsCached());
            AssertPoolFromMethodWithoutInspectingContext((container, method) =>
                container.BindPool<IService, ServicePool>()
                    .WithoutGameObjectActivation()
                    .FromMethod(method)
                    .AsCached());
            AssertPoolFromMethodWithoutInspectingContext((container, method) =>
                container.BindPool<IService, ServicePool>()
                    .To<Service>()
                    .FromMethod(method)
                    .AsCached());
        }

        [Test]
        public void ContextualFromMethod_IsAvailableOnEveryFluentSurface()
        {
            AssertContextualRegularFromMethod((container, method) =>
                container.Bind<IService>().To<Service>().FromMethod(method).AsTransient());
            AssertContextualRegularFromMethod((container, method) =>
                container.Bind<IService>().FromMethod(method).AsTransient());
            AssertContextualRegularFromMethod((container, method) =>
                container.Bind(typeof(IService)).FromMethod<Service>(method).AsTransient());

            AssertContextualFactoryFromMethod((container, method) =>
                container.BindFactory<IService, ServiceFactory>().FromMethod(method).AsCached());
            AssertContextualFactoryFromMethod((container, method) =>
                container.BindFactory<IService, ServiceFactory>()
                    .To<Service>()
                    .FromMethod(method)
                    .AsCached());

            AssertContextualParameterizedFactoryFromMethod((container, method) =>
                container.BindFactory<string, IService, ParameterizedServiceFactory>()
                    .FromMethod(method)
                    .AsCached());
            AssertContextualParameterizedFactoryFromMethod((container, method) =>
                container.BindFactory<string, IService, ParameterizedServiceFactory>()
                    .To<Service>()
                    .FromMethod(method)
                    .AsCached());

            AssertContextualPoolFromMethod((container, method) =>
                container.BindPool<IService, ServicePool>().FromMethod(method).AsCached());
            AssertContextualPoolFromMethod((container, method) =>
                container.BindPool<IService, ServicePool>()
                    .WithInitialSize(0)
                    .FromMethod(method)
                    .AsCached());
            AssertContextualPoolFromMethod((container, method) =>
                container.BindPool<IService, ServicePool>()
                    .WithMaxSize(2)
                    .FromMethod(method)
                    .AsCached());
            AssertContextualPoolFromMethod((container, method) =>
                container.BindPool<IService, ServicePool>()
                    .ExpandByOne()
                    .FromMethod(method)
                    .AsCached());
            AssertContextualPoolFromMethod((container, method) =>
                container.BindPool<IService, ServicePool>()
                    .WithoutGameObjectActivation()
                    .FromMethod(method)
                    .AsCached());
            AssertContextualPoolFromMethod((container, method) =>
                container.BindPool<IService, ServicePool>()
                    .To<Service>()
                    .FromMethod(method)
                    .AsCached());
        }

        [Test]
        public void ResolveWithContext_ForwardsOriginalContextFromCustomGetter()
        {
            var expected = new Service();
            var receivedContext = default(InjectContext);
            var container = new Container();
            container.Bind<Service>().FromMethod(context =>
            {
                receivedContext = context;
                return expected;
            }).AsTransient();
            var forwardingGetter = new ForwardingGetter(container);
            container.Bind<IService>().To<Service>().FromMethod(context =>
            {
                return (Service)forwardingGetter.GetInstance(
                    typeof(Service), CreateOptions.Default, context);
            }).AsTransient();
            var consumer = new MethodConsumer();

            container.Inject(consumer);

            Assert.That(consumer.Service, Is.SameAs(expected));
            Assert.That(receivedContext.ContractType, Is.EqualTo(typeof(IService)));
            Assert.That(receivedContext.ConsumerInstance, Is.SameAs(consumer));
            Assert.That(receivedContext.ConsumerType, Is.EqualTo(typeof(MethodConsumer)));
        }

        [Test]
        public void ResolveWithContext_WhenContextIsDefault_ThrowsArgumentException()
        {
            var container = new Container();
            container.Bind<Service>().AsTransient();
            var forwardingGetter = new ForwardingGetter(container);

            Assert.That(
                () => forwardingGetter.GetInstance(
                    typeof(Service),
                    CreateOptions.Default,
                    default),
                Throws.TypeOf<ArgumentException>());
        }
    }
}
