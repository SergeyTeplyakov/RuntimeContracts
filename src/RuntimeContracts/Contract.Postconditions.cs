using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace System.Diagnostics.ContractsLight;

public static partial class Contract
{
    /// <summary>
    /// Specifies a public contract such that the expression <paramref name="condition"/> will be true when the enclosing method or property returns normally.
    /// </summary>
    /// <param name="condition">Boolean expression representing the contract.  May include <seealso cref="OldValue{T}"/> and <seealso cref="Result{T}"/>.</param>
    /// <param name="userMessage">If it is not a constant string literal, then the contract may not be understood by tools.</param>
    /// <remarks>
    /// This call must happen at the beginning of a method or property before any other code.
    /// This contract is exposed to clients so must only reference members at least as visible as the enclosing method.
    /// The contract rewriter must be used for runtime enforcement of this postcondition.
    /// </remarks>
    [Pure]
    [Conditional("CONTRACTS_LIGHT_POSTCONDITIONS")]
    // [Obsolete("Not supported by RuntimeContracts")]
    public static void Ensures(bool condition, string userMessage = null)
    {
        // Doing nothing for now.
    }

    /// <summary>
    /// Speicifes a contract such that all elements in the <paramref name="collection"/> returns true for the given <paramref name="predicate"/>.
    /// </summary>
    [Pure]
    [Conditional("CONTRACTS_LIGHT_POSTCONDITIONS")]
    [Conditional("CONTRACTS_LIGHT_QUANTIFIERS")]
    // [Obsolete("Not supported by RuntimeContracts")]
    public static void EnsuresForAll<T>(IEnumerable<T> collection, Predicate<T> predicate, string userMessage = null)
    {
        // Doing nothing for now.
    }

    /// <summary>
    /// Specifies a contract such that if an exception of type <typeparamref name="TException"/> is thrown then the expression <paramref name="condition"/> will be true when the enclosing method or property terminates abnormally.
    /// </summary>
    [Pure]
    [Conditional("CONTRACTS_LIGHT_POSTCONDITIONS")]
    [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Exception type used in tools.")]
    // [Obsolete("Not supported by RuntimeContracts")]
    public static void EnsuresOnThrow<TException>(bool condition, string message = null) where TException : Exception
    {
        // Doing nothing for now.
    }

    /// <summary>
    /// Represents the result (a.k.a. return value) of a method or property.
    /// </summary>
    /// <typeparam name="T">Type of return value of the enclosing method or property.</typeparam>
    /// <returns>Return value of the enclosing method or property.</returns>
    /// <remarks>
    /// This method can only be used within the argument to the <seealso cref="Ensures(bool, string)"/> contract.
    /// </remarks>
    [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Not intended to be called at runtime.")]
    [Pure]
    // [Obsolete("Not supported by RuntimeContracts")]
    public static T Result<T>() { return default(T); }

    /// <summary>
    /// Represents the value of <paramref name="value"/> as it was at the start of the method or property.
    /// </summary>
    /// <typeparam name="T">Type of <paramref name="value"/>.  This can be inferred.</typeparam>
    /// <param name="value">Value to represent.  This must be a field or parameter.</param>
    /// <returns>Value of <paramref name="value"/> at the start of the method or property.</returns>
    /// <remarks>
    /// This method can only be used within the argument to the <seealso cref="Ensures(bool, string)"/> contract.
    /// </remarks>
    [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value")]
    [Pure]
    // [Obsolete("Not supported by RuntimeContracts")]
    public static T OldValue<T>(T value) { return default(T); }

    /// <summary>
    /// Represents the final (output) value of an out parameter when returning from a method.
    /// </summary>
    /// <typeparam name="T">Type of the out parameter.</typeparam>
    /// <param name="value">The out parameter.</param>
    /// <returns>The output value of the out parameter.</returns>
    /// <remarks>
    /// This method can only be used within the argument to the <seealso cref="Ensures(bool, string)"/> contract.
    /// </remarks>
    [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "0#", Justification = "Not intended to be called at runtime.")]
    [Pure]
    // [Obsolete("Not supported by RuntimeContracts")]
    public static T ValueAtReturn<T>(out T value) { value = default(T); return value; }
}