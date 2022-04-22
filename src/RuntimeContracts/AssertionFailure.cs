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
        internal string ConditionText { get; }

        internal AssertionFailure(string path, int lineNumber, string conditionText)
        {
            Path = path;
            LineNumber = lineNumber;
            ConditionText = conditionText;
        }
    }

    /// <summary>
    /// Represents an assertion violation.
    /// </summary>
    public readonly struct AssertionDebugFailure
    {
        internal string Path { get; }
        internal int LineNumber { get; }
        internal string ConditionText { get; }

        internal AssertionDebugFailure(string path, int lineNumber, string conditionText)
        {
            Path = path;
            LineNumber = lineNumber;
            ConditionText = conditionText;
        }
    }
}
