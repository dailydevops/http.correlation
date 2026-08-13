namespace NetEvolve.Http.Correlation.Ulid.ByteAether;

using global::ByteAether.Ulid;
using NetEvolve.Http.Correlation.Abstractions;

/// <inheritdoc />
internal sealed class UlidCorrelationIdProvider : IHttpCorrelationIdProvider
{
    /// <inheritdoc />
    public string GenerateId() => Ulid.New().ToString(format: null, System.Globalization.CultureInfo.InvariantCulture);
}
