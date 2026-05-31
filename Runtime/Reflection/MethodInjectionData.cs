using System.Reflection;

public readonly struct MethodInjectionData
{
    public readonly MethodInfo methodInfo;
    public readonly ParameterInfo[] parametersInfo;
    public readonly bool hasInjectMethod;

    public MethodInjectionData(MethodInfo methodInfo, ParameterInfo[] parametersInfo, bool hasInjectMethod)
    {
        this.methodInfo = methodInfo;
        this.parametersInfo = parametersInfo;
        this.hasInjectMethod = hasInjectMethod;
    }
}