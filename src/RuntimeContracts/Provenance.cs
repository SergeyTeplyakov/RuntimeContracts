using System.Globalization;

namespace System.Diagnostics.ContractsLight;

/// <summary>
/// Represents path and line of the violated assertion.
/// </summary>
internal readonly struct Provenance
{
    /// <summary>
    /// Path to the file that contains a violated assertion.
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// Line that contains a violated assertion.
    /// </summary>
    public int Line { get; }

    /// <nodoc />
    public Provenance(string path, int line)
    {
        Path = path;
        Line = line;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return string.Format(CultureInfo.InvariantCulture, "{0}:{1}", Path, Line.ToString());
    }
}