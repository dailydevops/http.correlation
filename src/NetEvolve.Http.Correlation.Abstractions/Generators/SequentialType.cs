namespace NetEvolve.Http.Correlation.Generators;

using System;

/// <summary>
/// Describes the position of the sequential part of the <see cref="Guid"/>.
/// </summary>
public enum SequentialType
{
    /// <summary>
    /// The sequential part is at the beginning of the <see cref="Guid"/>.
    /// </summary>
    /// <remarks>
    /// Used by Oracle.
    /// </remarks>
    AsBinary = 1,

    /// <summary>
    /// The sequential part is at the beginning of the <see cref="Guid"/>.
    /// </summary>
    /// <remarks>
    /// Used by MySql and PostgreSql.
    /// </remarks>
    AsString = 0,

    /// <summary>
    /// The sequential part is at the end of the <see cref="Guid"/>.
    /// </summary>
    /// <remarks>
    /// Used by SqlServer.
    /// </remarks>
    AtEnd = 2
}
