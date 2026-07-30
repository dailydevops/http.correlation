namespace NetEvolve.Http.Correlation.Azure.Functions.Tests.Integration;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Azure.Functions.Worker;

// Dependency-free stand-in for IInvocationFeatures. The middleware reaches
// context.GetInvocationResult(), which asks Features for the internal Functions Worker type
// IFunctionBindingsFeature - a type this assembly has no visibility into and can never name.
// Get<T>() sidesteps that: T is a type parameter at this call site, so DispatchProxy.Create<T, ...>()
// can synthesize a stub for it purely from the runtime Type object the Worker SDK supplies via
// reflection, without this file ever spelling out the interface name.
internal sealed class TestInvocationFeatures : IInvocationFeatures
{
    private readonly Dictionary<Type, object> _features = [];

    public void Set<T>(T instance) => _features[typeof(T)] = instance!;

    public T? Get<T>()
    {
        if (_features.TryGetValue(typeof(T), out var existing))
        {
            return (T)existing;
        }

        if (typeof(T).IsInterface)
        {
            var stub = DispatchProxy.Create<T, NullReturningProxy>();
            _features[typeof(T)] = stub!;
            return stub;
        }

        return default;
    }

    public IEnumerator<KeyValuePair<Type, object>> GetEnumerator() => _features.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
