namespace System.Diagnostics.ContractsLight
{
    /// <summary>Specifies the type of contract that failed. </summary>
    /// <remarks>
    /// The declaration was copied from the mscorlib.
    /// </remarks>
    public enum ContractFailureKind
    {
        /// <summary>A <see cref="Contract.Requires" /> contract failed.</summary>
        Precondition,

        /// <summary>An <see cref="Contract.Ensures" /> contract failed. </summary>
        Postcondition,

        /// <summary>An <see cref="Contract.EnsuresOnThrow{T}(bool, string)" /> contract failed.</summary>
        PostconditionOnException,

        /// <summary>An <see cref="Contract.Invariant" /> contract failed.</summary>
        Invariant,

        /// <summary>An <see cref="Contract.Assume" /> contract failed.</summary>
        Assume,

        /// <summary>An <see cref="Contract.Assert" /> contract failed.</summary>
        Assert,
    }

    /// <summary>
    /// Set of extension methods for <see cref="ContractFailureKind"/> enum.
    /// </summary>
    public static class ContractFailureKindExtensions
    {
        /// <summary>
        /// Returns a string representation of an enum <see cref="ContractFailureKind"/>.
        /// </summary>
        public static string ToDisplayString(this ContractFailureKind failureKind)
        {
            return failureKind switch
            {
                ContractFailureKind.Precondition => "Precondition failed",
                ContractFailureKind.Postcondition => "Postcondition failed",
                ContractFailureKind.PostconditionOnException => "Postcondition failed after throwing an exception",
                ContractFailureKind.Invariant => "Invariant failed",
                ContractFailureKind.Assert => "Assertion failed",
                ContractFailureKind.Assume => "Assumption failed",
                _ => throw new ArgumentOutOfRangeException(nameof(failureKind), failureKind, null),
            };
        }
    }
}