#nullable enable

namespace System.Diagnostics.FluentContracts
{
    /// <summary>
    /// Represents an assertion violation.
    /// </summary>
    public readonly struct AssertionFailure
    {
        internal string Path { get; }
        internal int LineNumber { get; }

        internal AssertionFailure(string path, int lineNumber)
        {
            Path = path;
            LineNumber = lineNumber;
        }
    }
}
