namespace NetEvolve.Http.Correlation.Azure.Functions.Tests.Integration;

using System;
using System.Reflection;

public class NullReturningProxy : DispatchProxy
{
    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args) =>
        targetMethod?.ReturnType is { IsValueType: true } rt && rt != typeof(void)
            ? Activator.CreateInstance(rt)
            : null;
}
