using System;
using System.Reflection;

namespace Uniject
{
    public readonly struct InjectContext
    {
        public Type ContractType { get; }
        public Type ConsumerType { get; }
        public object ConsumerInstance { get; }
        public ParameterInfo ParameterInfo { get; }

        internal InjectContext(
            Type contractType,
            Type consumerType = null,
            object consumerInstance = null,
            ParameterInfo parameterInfo = null)
        {
            ContractType = contractType ?? throw new ArgumentNullException(nameof(contractType));
            ConsumerType = consumerType;
            ConsumerInstance = consumerInstance;
            ParameterInfo = parameterInfo;
        }

        internal static InjectContext CreateRoot(Type contractType) => new(contractType);

        internal static InjectContext CreateForConstructorParameter(
            ParameterInfo parameterInfo,
            Type consumerType)
        {
            if (parameterInfo == null)
                throw new ArgumentNullException(nameof(parameterInfo));

            if (consumerType == null)
                throw new ArgumentNullException(nameof(consumerType));

            return new InjectContext(parameterInfo.ParameterType, consumerType, null, parameterInfo);
        }

        internal static InjectContext CreateForMethodParameter(
            ParameterInfo parameterInfo,
            object consumerInstance)
        {
            if (parameterInfo == null)
                throw new ArgumentNullException(nameof(parameterInfo));

            if (consumerInstance == null)
                throw new ArgumentNullException(nameof(consumerInstance));

            return new InjectContext(
                parameterInfo.ParameterType,
                consumerInstance.GetType(),
                consumerInstance,
                parameterInfo);
        }

        internal void EnsureIsValid(string parameterName)
        {
            if (ContractType == null)
                throw new ArgumentException("Inject context must be initialized.", parameterName);
        }
    }
}
