namespace NetEvolve.Http.Correlation.Azure.Functions.Tests.Integration;

/// <summary>
/// The result of a test run through <see cref="FunctionsCorrelationMiddleware"/>.
/// </summary>
public sealed record TestRunResult(bool NextCalled, string CorrelationId, string HeaderName);
