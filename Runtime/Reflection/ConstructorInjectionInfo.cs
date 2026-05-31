using System;
using System.Collections.Generic;
using System.Reflection;

namespace Uniject.Reflection
{
    public readonly struct ConstructorInjectionData
    {
        public readonly ConstructorInfo constructorInfo;
        public readonly ParameterInfo[] parametersInfo;

        public ConstructorInjectionData(ConstructorInfo constructorInfo, ParameterInfo[] parametersInfo)
        {
            this.constructorInfo = constructorInfo;
            this.parametersInfo = parametersInfo;
        }
    }
}
