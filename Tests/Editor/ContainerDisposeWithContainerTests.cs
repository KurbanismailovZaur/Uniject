using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Uniject.Bindings;
using Uniject.Lifecycle;

namespace Uniject.Tests
{
    public class ContainerDisposeWithContainerTests
    {
        private interface IFirst { }
        private interface ISecond { }
        private interface IThird { }

        private sealed class ConstructorDisposable : IDisposable
        {
            public static int InstancesCount { get; private set; }
            public static ConstructorDisposable LastInstance { get; private set; }

            public int DisposeCallsCount { get; private set; }

            public ConstructorDisposable()
            {
                InstancesCount++;
                LastInstance = this;
            }

            public void Dispose() => DisposeCallsCount++;

            public static void Reset()
            {
                InstancesCount = 0;
                LastInstance = null;
            }
        }

        private sealed class NonDisposable
        {
            public static int InstancesCount { get; private set; }

            public NonDisposable() => InstancesCount++;

            public static void Reset() => InstancesCount = 0;
        }

        private sealed class DisposableEntryPoint : IEntryPoint, IDisposable
        {
            public static DisposableEntryPoint LastInstance { get; private set; }

            public int RunCallsCount { get; private set; }
            public int DisposeCallsCount { get; private set; }

            public DisposableEntryPoint() => LastInstance = this;

            public void Run() => RunCallsCount++;

            public void Dispose() => DisposeCallsCount++;

            public static void Reset() => LastInstance = null;
        }

        private sealed class TrackingDisposable : IFirst, ISecond, IThird, IDisposable
        {
            private readonly string _name;
            private readonly ICollection<string> _disposeOrder;
            private readonly Action _onDispose;
            private readonly Exception _exceptionToThrow;

            public int DisposeCallsCount { get; private set; }

            public TrackingDisposable(
                string name = null,
                ICollection<string> disposeOrder = null,
                Action onDispose = null,
                Exception exceptionToThrow = null)
            {
                _name = name;
                _disposeOrder = disposeOrder;
                _onDispose = onDispose;
                _exceptionToThrow = exceptionToThrow;
            }

            public void Dispose()
            {
                DisposeCallsCount++;

                if (_name != null)
                    _disposeOrder?.Add(_name);

                _onDispose?.Invoke();

                if (_exceptionToThrow != null)
                    throw _exceptionToThrow;
            }
        }

        [SetUp]
        public void SetUp()
        {
            ConstructorDisposable.Reset();
            NonDisposable.Reset();
            DisposableEntryPoint.Reset();
        }

        [Test]
        public void DisposeWithContainer_CachedLazyBinding_DisposesResolvedInstance()
        {
            var container = new Container();
            container.Bind<ConstructorDisposable>()
                .AsCached()
                .DisposeWithContainer();

            Assert.That(ConstructorDisposable.InstancesCount, Is.Zero);

            var instance = container.Resolve<ConstructorDisposable>();
            container.Dispose();

            Assert.That(ConstructorDisposable.InstancesCount, Is.EqualTo(1));
            Assert.That(instance.DisposeCallsCount, Is.EqualTo(1));
        }

        [Test]
        public void DisposeWithContainer_CachedNonLazyBinding_DisposesInstanceCreatedByBuild()
        {
            var container = new Container();
            container.Bind<ConstructorDisposable>()
                .AsCached()
                .NonLazy()
                .DisposeWithContainer();

            container.Build();

            Assert.That(ConstructorDisposable.InstancesCount, Is.EqualTo(1));
            Assert.That(ConstructorDisposable.LastInstance, Is.Not.Null);

            container.Dispose();

            Assert.That(ConstructorDisposable.LastInstance.DisposeCallsCount, Is.EqualTo(1));
        }

        [Test]
        public void DisposeWithContainer_CachedEntryPointBinding_RunsAndDisposesInstance()
        {
            var container = new Container();
            container.Bind<DisposableEntryPoint>()
                .AsCached()
                .NonLazy()
                .AsEntryPoint()
                .DisposeWithContainer();

            container.Build();

            Assert.That(DisposableEntryPoint.LastInstance, Is.Not.Null);
            Assert.That(DisposableEntryPoint.LastInstance.RunCallsCount, Is.EqualTo(1));

            container.Dispose();

            Assert.That(DisposableEntryPoint.LastInstance.DisposeCallsCount, Is.EqualTo(1));
        }

        [Test]
        public void DisposeWithContainer_BindInstance_DisposesBeforeBuild()
        {
            var container = new Container();
            var instance = new TrackingDisposable();

            container.BindInstance(instance).DisposeWithContainer();
            container.Dispose();

            Assert.That(container.IsBuilded, Is.False);
            Assert.That(instance.DisposeCallsCount, Is.EqualTo(1));
        }

        [Test]
        public void Dispose_UnmarkedCachedBinding_DoesNotDisposeInstance()
        {
            var container = new Container();
            container.Bind<ConstructorDisposable>().AsCached();

            var instance = container.Resolve<ConstructorDisposable>();
            container.Dispose();

            Assert.That(instance.DisposeCallsCount, Is.Zero);
        }

        [Test]
        public void DisposeWithContainer_UncreatedLazyConstructorBinding_DoesNotInstantiateInstance()
        {
            var container = new Container();
            container.Bind<ConstructorDisposable>()
                .AsCached()
                .DisposeWithContainer();

            container.Dispose();

            Assert.That(ConstructorDisposable.InstancesCount, Is.Zero);
            Assert.That(ConstructorDisposable.LastInstance, Is.Null);
        }

        [Test]
        public void Resolve_DisposeWithContainerForNonDisposable_ThrowsInvalidOperationExceptionAtRuntime()
        {
            var container = new Container();

            Assert.That(
                () => container.Bind<NonDisposable>().AsCached().DisposeWithContainer(),
                Throws.Nothing);

            Assert.That(
                () => container.Resolve<NonDisposable>(),
                Throws.TypeOf<InvalidOperationException>());
            Assert.That(NonDisposable.InstancesCount, Is.EqualTo(1));

            container.Dispose();
        }

        [Test]
        public void Resolve_DisposeWithContainerForNullResult_ThrowsInvalidOperationException()
        {
            var container = new Container();
            container.Bind<IDisposable>()
                .FromMethod(_ => (IDisposable)null)
                .AsCached()
                .DisposeWithContainer();

            Assert.That(
                () => container.Resolve<IDisposable>(),
                Throws.TypeOf<InvalidOperationException>());

            container.Dispose();
        }

        [Test]
        public void Dispose_SameMarkedReferenceBoundSeveralTimes_DisposesOnce()
        {
            var container = new Container();
            var instance = new TrackingDisposable();

            container.Bind<IFirst>().FromInstance(instance).AsCached().DisposeWithContainer();
            container.Bind<ISecond>().FromInstance(instance).AsCached().DisposeWithContainer();

            Assert.That(container.Resolve<IFirst>(), Is.SameAs(instance));
            Assert.That(container.Resolve<ISecond>(), Is.SameAs(instance));

            container.Dispose();

            Assert.That(instance.DisposeCallsCount, Is.EqualTo(1));
        }

        [Test]
        public void Dispose_MarkedInstances_DisposesInReverseRegistrationOrder()
        {
            var disposeOrder = new List<string>();
            var first = new TrackingDisposable("first", disposeOrder);
            var second = new TrackingDisposable("second", disposeOrder);
            var third = new TrackingDisposable("third", disposeOrder);
            var container = new Container();

            container.Bind<IFirst>().FromInstance(first).AsCached().DisposeWithContainer();
            container.Bind<ISecond>().FromInstance(second).AsCached().DisposeWithContainer();
            container.Bind<IThird>().FromInstance(third).AsCached().DisposeWithContainer();

            container.Dispose();

            Assert.That(disposeOrder, Is.EqualTo(new[] { "third", "second", "first" }));
        }

        [Test]
        public void Dispose_WhenCalledReentrantlyAndRepeatedly_DisposesInstanceOnce()
        {
            var container = new Container();
            var instance = new TrackingDisposable(onDispose: container.Dispose);
            container.BindInstance(instance).DisposeWithContainer();

            Assert.That(() => container.Dispose(), Throws.Nothing);
            Assert.That(() => container.Dispose(), Throws.Nothing);

            Assert.That(instance.DisposeCallsCount, Is.EqualTo(1));
        }

        [Test]
        public void Dispose_WhenOneInstanceThrows_ContinuesCleanupAndThrowsAggregateException()
        {
            var disposeOrder = new List<string>();
            var first = new TrackingDisposable("first", disposeOrder);
            var failing = new TrackingDisposable(
                "failing",
                disposeOrder,
                exceptionToThrow: new InvalidOperationException("dispose failed"));
            var last = new TrackingDisposable("last", disposeOrder);
            var container = new Container();

            container.Bind<IFirst>().FromInstance(first).AsCached().DisposeWithContainer();
            container.Bind<ISecond>().FromInstance(failing).AsCached().DisposeWithContainer();
            container.Bind<IThird>().FromInstance(last).AsCached().DisposeWithContainer();

            var exception = Assert.Throws<AggregateException>(() => container.Dispose());

            Assert.That(disposeOrder, Is.EqualTo(new[] { "last", "failing", "first" }));
            Assert.That(first.DisposeCallsCount, Is.EqualTo(1));
            Assert.That(failing.DisposeCallsCount, Is.EqualTo(1));
            Assert.That(last.DisposeCallsCount, Is.EqualTo(1));
            Assert.That(exception.InnerExceptions, Has.Count.EqualTo(1));
            Assert.That(exception.InnerExceptions[0], Is.TypeOf<InvalidOperationException>());
            Assert.That(exception.InnerExceptions[0].Message, Is.EqualTo("dispose failed"));
        }

        [Test]
        public void Operations_AfterDispose_ThrowObjectDisposedException()
        {
            var container = new Container();
            var retainedBuilder = container.Bind<ConstructorDisposable>().AsCached();
            container.Dispose();

            Assert.That(
                () => container.Bind<ConstructorDisposable>(),
                Throws.TypeOf<ObjectDisposedException>());
            Assert.That(
                () => container.Resolve<ConstructorDisposable>(),
                Throws.TypeOf<ObjectDisposedException>());
            Assert.That(
                () => container.Build(),
                Throws.TypeOf<ObjectDisposedException>());
            Assert.That(
                () => retainedBuilder.DisposeWithContainer(),
                Throws.TypeOf<ObjectDisposedException>());
        }

        [Test]
        public void DisposeWithContainer_FinalizesCachedBuilderConfiguration()
        {
            var container = new Container();
            var fromBuilder = container.Bind<IFirst>().To<TrackingDisposable>();
            var asBuilder = fromBuilder.FromConstructor();
            var cachedBuilder = asBuilder.AsCached();

            cachedBuilder.DisposeWithContainer();

            Assert.That(() => asBuilder.AsTransient(), Throws.TypeOf<InvalidOperationException>());
            Assert.That(() => asBuilder.AsCached(), Throws.TypeOf<InvalidOperationException>());
            Assert.That(() => cachedBuilder.NonLazy(), Throws.TypeOf<InvalidOperationException>());
            Assert.That(
                () => fromBuilder.FromInstance(new TrackingDisposable()),
                Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void FluentApi_ExposesDisposeOnlyOnCachedTerminalStages()
        {
            AssertTerminalDisposeMethod(
                typeof(BindingToTypeCachedBuilder),
                "NonLazy",
                "DisposeWithContainer");
            AssertTerminalDisposeMethod(
                typeof(BindingToTypeCachedNonLazyBuilder),
                "AsEntryPoint",
                "DisposeWithContainer");
            AssertTerminalDisposeMethod(
                typeof(BindingToTypeCachedEntryPointBuilder),
                "DisposeWithContainer");

            AssertDoesNotExposeDispose(typeof(BindingToTypeAsBuilder));
            AssertDoesNotExposeDispose(typeof(BindingToTypeNonLazyBuilder));
            AssertDoesNotExposeDispose(typeof(BindingToTypeAsEntryPointBuilder));

            AssertSubcontainerAsCachedReturnsNonDisposableBuilder(typeof(BindingToSubcontainerAsBuilder));
            AssertSubcontainerAsCachedReturnsNonDisposableBuilder(typeof(BindingToSubcontainerUnderTransformBuilder));
            AssertSubcontainerAsCachedReturnsNonDisposableBuilder(typeof(BindingToSubcontainerWithGameObjectNameBuilder));
        }

        private static void AssertTerminalDisposeMethod(Type builderType, params string[] expectedDeclaredMethods)
        {
            var disposeMethod = builderType.GetMethod(
                "DisposeWithContainer",
                BindingFlags.Instance | BindingFlags.Public);

            Assert.That(disposeMethod, Is.Not.Null, $"{builderType} must expose DisposeWithContainer.");
            Assert.That(disposeMethod.DeclaringType, Is.EqualTo(builderType));
            Assert.That(disposeMethod.ReturnType, Is.EqualTo(typeof(void)));
            Assert.That(disposeMethod.GetParameters(), Is.Empty);

            var declaredMethods = builderType
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Select(method => method.Name)
                .OrderBy(methodName => methodName)
                .ToArray();

            Assert.That(
                declaredMethods,
                Is.EqualTo(expectedDeclaredMethods.OrderBy(methodName => methodName).ToArray()));
        }

        private static void AssertDoesNotExposeDispose(Type builderType)
        {
            Assert.That(
                builderType.GetMethod("DisposeWithContainer", BindingFlags.Instance | BindingFlags.Public),
                Is.Null,
                $"{builderType} must not expose DisposeWithContainer.");
        }

        private static void AssertSubcontainerAsCachedReturnsNonDisposableBuilder(Type builderType)
        {
            var asCachedMethod = builderType.GetMethod(
                "AsCached",
                BindingFlags.Instance | BindingFlags.Public);

            Assert.That(asCachedMethod, Is.Not.Null, $"{builderType} must expose AsCached.");
            Assert.That(asCachedMethod.ReturnType, Is.EqualTo(typeof(BindingToTypeNonLazyBuilder)));
            AssertDoesNotExposeDispose(asCachedMethod.ReturnType);
        }
    }
}
