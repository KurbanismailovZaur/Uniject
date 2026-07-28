using System;
using System.Reflection;

namespace Uniject
{
    public readonly struct InjectContext
    {
        public Container Container { get; }
        public Type ContractType { get; }
        public Type ConsumerType { get; }
        public object ConsumerInstance { get; }
        public ParameterInfo ParameterInfo { get; }

        internal InjectContext(
            Container container,
            Type contractType,
            Type consumerType = null,
            object consumerInstance = null,
            ParameterInfo parameterInfo = null)
        {
            Container = container ?? throw new ArgumentNullException(nameof(container));
            ContractType = contractType ?? throw new ArgumentNullException(nameof(contractType));
            ConsumerType = consumerType;
            ConsumerInstance = consumerInstance;
            ParameterInfo = parameterInfo;
        }

        internal static InjectContext CreateRoot(Container container, Type contractType) =>
            new(container, contractType);

        internal static InjectContext CreateForConstructorParameter(
            Container container,
            ParameterInfo parameterInfo,
            Type consumerType)
        {
            if (parameterInfo == null)
                throw new ArgumentNullException(nameof(parameterInfo));

            if (consumerType == null)
                throw new ArgumentNullException(nameof(consumerType));

            return new InjectContext(container, parameterInfo.ParameterType, consumerType, null, parameterInfo);
        }

        internal static InjectContext CreateForMethodParameter(
            Container container,
            ParameterInfo parameterInfo,
            object consumerInstance)
        {
            if (parameterInfo == null)
                throw new ArgumentNullException(nameof(parameterInfo));

            if (consumerInstance == null)
                throw new ArgumentNullException(nameof(consumerInstance));

            return new InjectContext(
                container,
                parameterInfo.ParameterType,
                consumerInstance.GetType(),
                consumerInstance,
                parameterInfo);
        }

        internal InjectContext WithContainer(Container container) =>
            new(container, ContractType, ConsumerType, ConsumerInstance, ParameterInfo);

        internal void EnsureIsValid(string parameterName)
        {
            if (Container == null || ContractType == null)
                throw new ArgumentException("Inject context must be initialized.", parameterName);
        }
    }
}
