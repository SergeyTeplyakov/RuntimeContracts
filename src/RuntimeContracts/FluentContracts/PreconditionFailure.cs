#nullable enable

namespace System.Diagnostics.FluentContracts
{
    /// <summary>
    /// Represents a precondition violation.
    /// </summary>
    public readonly struct PreconditionFailure
    {
        internal string Path { get; }
        internal int LineNumber { get; }

        internal PreconditionFailure(string path, int lineNumber)
        {
            Path = path;
            LineNumber = lineNumber;
        }
    }
}
