using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Uniject;
using Uniject.Exceptions;
using Uniject.Lifecycle;

namespace Uniject.Tests
{
    public class ContainerEntryPointsTests
    {
        private class EntryPointClass : IEntryPoint
        {
            public static int InstancesCount { get; set; }
            public static int StartsCount { get; set; }

            public EntryPointClass() => InstancesCount++;

            public void Start() => StartsCount++;
        }

        private class NotEntryPoint { }

        private static class EntryPointOrder
        {
            public static readonly List<string> Items = new();
            public static void Clear() => Items.Clear();
        }

        private class FirstEntryPoint : IEntryPoint
        {
            public void Start() => EntryPointOrder.Items.Add("First");
        }

        private class SecondEntryPoint : IEntryPoint
        {
            public void Start() => EntryPointOrder.Items.Add("Second");
        }

        private static void ResolveNonLazyBindings(Container container)
        {
            var method = typeof(Container).GetMethod("ResolveNonLazyBindings", BindingFlags.Instance | BindingFlags.NonPublic);
            method.Invoke(container, null);
        }

        private static void CallEntryPoints(Container container)
        {
            var method = typeof(Container).GetMethod("CallEntryPoints", BindingFlags.Instance | BindingFlags.NonPublic);
            method.Invoke(container, null);
        }

        [Test]
        public void CallEntryPoints_WhenBindingIsEntryPoint_CallsStart()
        {
            EntryPointClass.InstancesCount = 0;
            EntryPointClass.StartsCount = 0;

            var container = new Container();
            container.Bind<EntryPointClass>().AsEntryPoint();

            ResolveNonLazyBindings(container);
            CallEntryPoints(container);

            Assert.That(EntryPointClass.InstancesCount, Is.EqualTo(1));
            Assert.That(EntryPointClass.StartsCount, Is.EqualTo(1));
        }

        [Test]
        public void CallEntryPoints_WhenBindingIsOnlyNonLazy_DoesNotCallStart()
        {
            EntryPointClass.StartsCount = 0;

            var container = new Container();
            container.Bind<EntryPointClass>().NonLazy();

            ResolveNonLazyBindings(container);
            CallEntryPoints(container);

            Assert.That(EntryPointClass.StartsCount, Is.EqualTo(0));
        }

        [Test]
        public void CallEntryPoints_WhenSeveralEntryPointsExist_CallsStartInRegistrationOrder()
        {
            EntryPointOrder.Clear();

            var container = new Container();
            container.Bind<FirstEntryPoint>().AsEntryPoint();
            container.Bind<SecondEntryPoint>().AsEntryPoint();

            ResolveNonLazyBindings(container);
            CallEntryPoints(container);

            Assert.That(EntryPointOrder.Items, Is.EqualTo(new[] { "First", "Second" }));
        }

        [Test]
        public void AsEntryPoint_WhenConcreteTypeDoesNotImplementIEntryPoint_ThrowsBindingException()
        {
            var container = new Container();
            Assert.That(() => container.Bind<NotEntryPoint>().AsEntryPoint(), Throws.TypeOf<BindingException>());
        }
    }
}
