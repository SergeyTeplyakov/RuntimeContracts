using System.Runtime.CompilerServices;
using System.Text;

#nullable enable

namespace System.Diagnostics.ContractsLight
{
    /// <summary>
    /// A special interpolated string handler used for lazily creating a user-provided assertion violation messages.
    /// </summary>
    /// <remarks>
    /// Using the improved interpolated string feature added to C# 10 allows creating a user-defined string only when the assertion is violated.
    /// </remarks>
    [InterpolatedStringHandler]
    public ref struct ContractMessageInterpolatedStringHandler
    {
        /// <summary>
        /// Lazily allocated string builder for creating an error string.
        /// </summary>
        /// <remarks>
        /// It's fine to use a newly created builder each time when the assertion is violated, because we know that it should not be happening very often.
        /// </remarks>
        private readonly StringBuilder? _builder;

        /// <summary>
        /// A constructor that the compiler calls for creating an interpolated string.
        /// </summary>
        public ContractMessageInterpolatedStringHandler(int literalLength, int formattedCount, bool predicate, out bool handlerIsValid)
        {
            // Completely ignoring the first two arguments that are required by the compiler.
            // But we don't need them because the contract violations must happen very infrequently and the fact that the string construction in that case is not
            // super efficient is not important.
            _builder = null;

            if (predicate)
            {
                // The assersion is not violated. Not creating a string at all.
                handlerIsValid = false;
                return;
            }

            handlerIsValid = true;
            _builder = new StringBuilder();
        }

        /// <summary>
        /// Appends a given <paramref name="s"/> into a final message.
        /// </summary>
        public void AppendLiteral(string s) => _builder!.Append(s);

        /// <summary>
        /// Appends a given <paramref name="t"/> to a final message.
        /// </summary>
        public void AppendFormatted<T>(T t) => _builder!.Append(t?.ToString());

        /// <inheritdoc />
        public override string ToString() => _builder!.ToString();
    }
}