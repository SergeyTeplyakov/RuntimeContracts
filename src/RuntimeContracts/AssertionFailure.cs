#nullable enable

namespace System.Diagnostics.ContractsLight
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

    /// <summary>
    /// Represents an assertion violation.
    /// </summary>
    public readonly struct AssertionDebugFailure
    {
        internal string Path { get; }
        internal int LineNumber { get; }

        internal AssertionDebugFailure(string path, int lineNumber)
        {
            Path = path;
            LineNumber = lineNumber;
        }
    }
}
