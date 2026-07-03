using Uniject.Tests.Fixtures;
using UnityEngine;

namespace Uniject.Tests
{
    public abstract class ContainerFactoryTestFixture
    {
        protected interface IProduct { }

        protected class Product : IProduct { }

        protected class ProductFactory : Factory<Product> { }

        protected class InterfaceProductFactory : Factory<IProduct> { }

        protected class ScriptFactory : Factory<Script> { }

        protected class InjectableScriptFactory : Factory<InjectableScript> { }

        protected class GameObjectScriptFactory : Factory<GameObject, Script> { }

        protected class ScriptScriptFactory : Factory<Script, Script> { }

        protected class TransformScriptFactory : Factory<Transform, Script> { }

        protected class InterfaceScriptFactory : Factory<IInterface, Script> { }

        protected class GameObjectInterfaceFactory : Factory<GameObject, IInterface> { }

        protected class InterfaceInterfaceFactory : Factory<IInterface, IInterface> { }

        protected class GameObjectInjectableScriptFactory : Factory<GameObject, InjectableScript> { }

        protected class FloatScriptFactory : Factory<float, Script> { }

        protected class GameObjectProductFactory : Factory<GameObject, Product> { }

        protected class GameObjectIProductFactory : Factory<GameObject, IProduct> { }

        protected class ClassProductFactory : Factory<Class, Product> { }

        protected class CustomProductFactory : CustomFactory<Product>
        {
            public override Product Create()
            {
                return _container.Instantiate<Product>();
            }
        }

        protected class InitializableCustomProductFactory : CustomFactory<Product>
        {
            public static int InitializeCallsCount { get; private set; }
            public static int CreateCallsCount { get; private set; }
            public static Class ResolvedDependency { get; private set; }
            public static bool WasInitializedBeforeCreate { get; private set; }

            public static void Reset()
            {
                InitializeCallsCount = 0;
                CreateCallsCount = 0;
                ResolvedDependency = null;
                WasInitializedBeforeCreate = false;
            }

            protected override void Initialize()
            {
                InitializeCallsCount++;
                ResolvedDependency = _container.Resolve<Class>();
            }

            public override Product Create()
            {
                CreateCallsCount++;
                WasInitializedBeforeCreate = InitializeCallsCount > 0;
                return _container.Instantiate<Product>();
            }
        }

        protected class CustomScriptWithParameterFactory : CustomFactory<GameObject, Script>
        {
            public override Script Create(GameObject prefab)
            {
                return new GameObject(prefab.name).AddComponent<Script>();
            }
        }

        protected class CustomProductWithClassParameterFactory : CustomFactory<Class, Product>
        {
            public static Class LastParameter { get; private set; }

            public static void Reset() => LastParameter = null;

            public override Product Create(Class parameter)
            {
                LastParameter = parameter;
                return _container.Instantiate<Product>();
            }
        }

        protected class InitializableCustomScriptWithParameterFactory : CustomFactory<GameObject, Script>
        {
            public static int InitializeCallsCount { get; private set; }
            public static int CreateCallsCount { get; private set; }
            public static Class ResolvedDependency { get; private set; }
            public static bool WasInitializedBeforeCreate { get; private set; }

            public static void Reset()
            {
                InitializeCallsCount = 0;
                CreateCallsCount = 0;
                ResolvedDependency = null;
                WasInitializedBeforeCreate = false;
            }

            protected override void Initialize()
            {
                InitializeCallsCount++;
                ResolvedDependency = _container.Resolve<Class>();
            }

            public override Script Create(GameObject prefab)
            {
                CreateCallsCount++;
                WasInitializedBeforeCreate = InitializeCallsCount > 0;
                return new GameObject(prefab.name).AddComponent<Script>();
            }
        }

        protected class CustomInterfaceScriptWithParameterFactory : CustomFactory<GameObject, ScriptImplementedIInterface>
        {
            public override ScriptImplementedIInterface Create(GameObject prefab)
            {
                return new GameObject(prefab.name).AddComponent<ScriptImplementedIInterface>();
            }
        }

        protected class CustomScriptWithInterfaceParameterFactory : CustomFactory<IInterface, Script>
        {
            public override Script Create(IInterface prefab)
            {
                var prefabComponent = (Component)prefab;
                return new GameObject(prefabComponent.gameObject.name).AddComponent<Script>();
            }
        }

        protected class CustomInjectableScriptWithParameterFactory : CustomFactory<GameObject, InjectableScript>
        {
            public override InjectableScript Create(GameObject prefab)
            {
                var gameObject = new GameObject(prefab.name);
                return _container.AddComponent<InjectableScript>(gameObject);
            }
        }
    }
}
