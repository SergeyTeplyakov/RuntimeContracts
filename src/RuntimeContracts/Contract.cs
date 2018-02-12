using System.Collections.Generic;
#if NETSTANDARD2_0
using System.ComponentModel;
#endif
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace System.Diagnostics.ContractsLight
{
    /// <summary>
    /// Contains static methods for representing program contracts such as preconditions, postconditions, and invariants.
    /// </summary>
    /// <remarks>
    /// Unlike <see cref="System.Diagnostics.Contracts.Contract"/> the following class does not require any tools in the build pipeline.
    /// A user may control the "level" by defining following symbols:
    /// - CONTRACTS_LIGHT_PRECONDITIONS - to enable preconditions.
    /// - CONTRACTS_LIGHT_INVARIANTS - to "partially" enable invariants (Invariant method should be called manually).
    /// - CONTRACTS_LIGHT_ASSERTS to enable <see cref="Assert"/> and <see cref="Assume"/> methods.
    /// - CONTRACTS_LIGHT_QUANTIFIERS to enable quantifiers like <see cref="ForAll{T}"/> or <see cref="Exists{T}"/>.
    ///
    /// Postconditions are no ops in the current version because they require some code manipulation at runtime.
    /// Failed precondition/assertion/invariant triggers <see cref="ContractFailed"/> event and if the handler of that event will not "handle" the violation,
    /// <see cref="ContractException"/> will be thrown.
    ///
    /// Every assertion in this type takes two additional optional arguments: 'path' and 'lineNumber'.
    /// Unfortunately as of Sep 2017 csc can mess up with line numbers for async methods in release mode.
    /// To get the correct line and column information we need to use <see cref="CallerFilePathAttribute"/> and <see cref="CallerLineNumberAttribute"/> attributes.
    /// </remarks>
    public static partial class Contract
    {
        /// <summary>
        /// Specifies a contract such that the expression <paramref name="condition"/> must be true before the enclosing method or property is invoked.
        /// </summary>
        /// <param name="condition">Boolean expression representing the contract.</param>
        /// <param name="userMessage">User-provided error message.</param>
        /// <param name="path">Compiler generated path to the file with the assertion.</param>
        /// <param name="lineNumber">Compiler generated line number of the assertion.</param>
        /// <remarks>
        /// This call must happen at the beginning of a method or property before any other code.
        /// This contract is exposed to clients so must only reference members at least as visible as the enclosing method.
        /// Use this form when backward compatibility does not force you to throw a particular exception.
        /// </remarks>
        [Pure]
        [Conditional("CONTRACTS_LIGHT_PRECONDITIONS")]
        public static void Requires(
            bool condition, 
            string userMessage = null, 
            [CallerFilePath] string path = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            if (!condition)
            {
                ContractRuntimeHelper.ReportFailure(ContractFailureKind.Precondition, userMessage, null, new Provenance(path, lineNumber));
            }
        }
        
        /// <summary>
        /// Specifies a contract such that the expression <paramref name="condition"/> must be true before the enclosing method or property is invoked.
        /// </summary>
        /// <param name="condition">Boolean expression representing the contract.</param>
        /// <param name="userMessage">User-provided error message.</param>
        /// <param name="path">Compiler generated path to the file with the assertion.</param>
        /// <param name="lineNumber">Compiler generated line number of the assertion.</param>
        /// <remarks>
        /// This call must happen at the beginning of a method or property before any other code.
        /// This contract is exposed to clients so must only reference members at least as visible as the enclosing method.
        /// Use this form when backward compatibility does not force you to throw a particular exception.
        /// </remarks>
        [Pure]
        [Conditional("CONTRACTS_LIGHT_PRECONDITIONS")]
        public static void RequiresDebug(
            bool condition, 
            string userMessage = null, 
            [CallerFilePath] string path = "",
            [CallerLineNumber] int lineNumber = 0)
        {
#if DEBUG
            if (!condition)
            {
                ContractRuntimeHelper.ReportFailure(ContractFailureKind.Precondition, userMessage, null, new Provenance(path, lineNumber));
            }
#endif
        }

        /// <summary>
        /// Speicifes a contract such that 'predicate' returns true for each element in 'collection'.
        /// </summary>
        /// <remarks>
        /// Unlike stand alone <see cref="ForAll{T}"/> this method will not cause a boxing allocation (if the collection is a value type) when the quantifiers are disabled.
        /// </remarks>
        [Pure]
        [Conditional("CONTRACTS_LIGHT_QUANTIFIERS")]
        public static void RequiresForAll<T>(
            IEnumerable<T> collection, 
            Predicate<T> predicate,
            string userMessage = null, 
            [CallerFilePath] string path = "", 
            [CallerLineNumber] int lineNumber = 0)
        {
#if CONTRACTS_LIGHT_PRECONDITIONS
            if (!CheckForAll(collection, predicate))
            {
                ContractRuntimeHelper.ReportFailure(ContractFailureKind.Precondition, userMessage, null, new Provenance(path, lineNumber));
            }
#endif
        }

        /// <summary>
        /// Specifies a contract such that the expression <paramref name="condition"/> must be true before the enclosing method or property is invoked.
        /// </summary>
        /// <param name="condition">Boolean expression representing the contract.</param>
        /// <param name="userMessage">User-provided error message.</param>
        /// <param name="path">Compiler generated path to the file with the assertion.</param>
        /// <param name="lineNumber">Compiler generated line number of the assertion.</param>
        /// <remarks>
        /// This call must happen at the beginning of a method or property before any other code.
        /// This contract is exposed to clients so must only reference members at least as visible as the enclosing method.
        /// Use this form when you want to throw a particular exception.
        /// </remarks>
        /// <typeparam name="TException">Exception type that will be thrown if a precondition is failed.</typeparam>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "condition")]
        [Pure]
        [Conditional("CONTRACTS_LIGHT_PRECONDITIONS")]
        public static void Requires<TException>(
            bool condition,
#if NETSTANDARD2_0
            [Localizable(false)]
#endif
            string userMessage = null,
            [CallerFilePath] string path = "", 
            [CallerLineNumber] int lineNumber = 0) where TException : Exception
#if !NETSTANDARD2_0
            // Previous version are relies on new constraint for exception construction.
            , new()
#endif
        {
            if (!condition)
            {
                ContractRuntimeHelper.ReportPreconditionFailure<TException>(userMessage, null, new Provenance(path, lineNumber));
            }
        }

        /// <summary>
        /// Perform a runtime check that <paramref name="condition"/> is true.
        /// </summary>
        /// <param name="condition">Expression to check to always be true.</param>
        /// <param name="userMessage">User-provided error message.</param>
        /// <param name="path">Compiler generated path to the file with the assertion.</param>
        /// <param name="lineNumber">Compiler generated line number of the assertion.</param>
        [Pure]
        [Conditional("CONTRACTS_LIGHT_ASSERTS")]
        public static void Assert(
            bool condition,
#if NETSTANDARD2_0
            [Localizable(false)]
#endif
            string userMessage = null,
            [CallerFilePath] string path = null, 
            [CallerLineNumber] int lineNumber = 0)
        {
            if (!condition)
            {
                ContractRuntimeHelper.ReportFailure(ContractFailureKind.Assert, userMessage, null, new Provenance(path, lineNumber));
            }
        }
        
        /// <summary>
        /// Perform a runtime check that <paramref name="condition"/> is true.
        /// </summary>
        /// <param name="condition">Expression to check to always be true.</param>
        /// <param name="userMessage">User-provided error message.</param>
        /// <param name="path">Compiler generated path to the file with the assertion.</param>
        /// <param name="lineNumber">Compiler generated line number of the assertion.</param>
        [Pure]
        [Conditional("CONTRACTS_LIGHT_ASSERTS")]
        public static void AssertDebug(
            bool condition,
#if NETSTANDARD2_0
            [Localizable(false)]
#endif
            string userMessage = null,
            [CallerFilePath] string path = null, 
            [CallerLineNumber] int lineNumber = 0)
        {
#if DEBUG
            if (!condition)
            {
                ContractRuntimeHelper.ReportFailure(ContractFailureKind.Assert, userMessage, null, new Provenance(path, lineNumber));
            }
#endif
        }

        /// <summary>
        /// Helper method that throws <see cref="ContractException"/> uncodintionally.
        /// This allows e.g. <code>throw Contract.AssertFailure("Oh no!");</code>
        /// </summary>
        public static Exception AssertFailure(
            // Disable localization prevents CA2204
#if NETSTANDARD2_0
            [Localizable(false)]
#endif
            string message = null,
            [CallerFilePath] string path = null,
            [CallerLineNumber] int lineNumber = 0)
        {
            Assert(false);
            // This method should fail regardless of the ContractFailEvent handlers.
            ContractRuntimeHelper.RaiseContractFailedEvent(
                ContractFailureKind.Assert,
                message,
                null,
                new Provenance(path, lineNumber),
                out var text);

            return new ContractException(ContractFailureKind.Assert, text, message, null);
        }

        /// <summary>
        /// Perform a runtime check that <paramref name="condition"/> is true.
        /// </summary>
        /// <param name="condition">Expression to check to always be true.</param>
        /// <param name="userMessage">User-provided error message.</param>
        /// <param name="path">Compiler generated path to the file with the assertion.</param>
        /// <param name="lineNumber">Compiler generated line number of the assertion.</param>
        [Pure]
        [Conditional("CONTRACTS_LIGHT_ASSERTS")]
        public static void Assume(
            bool condition,
#if NETSTANDARD2_0
            [Localizable(false)]
#endif
            string userMessage = null,
            [CallerFilePath] string path = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            if (!condition)
            {
                ContractRuntimeHelper.ReportFailure(ContractFailureKind.Assume, userMessage, null, new Provenance(path, lineNumber));
            }
        }

        /// <summary>
        /// Specifies a contract such that the expression <paramref name="condition"/> will be true after every method or property on the enclosing class.
        /// </summary>
        /// <param name="condition">Boolean expression representing the contract.</param>
        /// <param name="userMessage">User-provided error message.</param>
        /// <param name="path">Compiler generated path to the file with the assertion.</param>
        /// <param name="lineNumber">Compiler generated line number of the assertion.</param>
        /// <remarks>
        /// This contact can only be specified in a dedicated invariant method declared on a class.
        /// This contract is not exposed to clients so may reference members less visible as the enclosing method.
        /// The contract rewriter must be used for runtime enforcement of this invariant.
        /// </remarks>
        [Pure]
        [Conditional("CONTRACTS_LIGHT_INVARIANTS")]
        public static void Invariant(
            bool condition,
#if NETSTANDARD2_0
            [Localizable(false)]
#endif
            string userMessage = null,
            [CallerFilePath] string path = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            if (!condition)
            {
                ContractRuntimeHelper.ReportFailure(ContractFailureKind.Invariant, userMessage, null, new Provenance(path, lineNumber));
            }
        }

        /// <summary>
        /// Allows a managed application environment such as an interactive interpreter (IronPython)
        /// to be notified of contract failures and 
        /// potentially "handle" them, either by throwing a particular exception type, etc.  If any of the
        /// event handlers sets the Cancel flag in the ContractFailedEventArgs, then the Contract class will
        /// not pop up an assert dialog box or trigger escalation policy.  Hooking this event requires 
        /// full trust, because it will inform you of bugs in the appdomain and because the event handler
        /// could allow you to continue execution.
        /// </summary>
        public static event EventHandler<ContractFailedEventArgs> ContractFailed
        {
            add { ContractRuntimeHelper.ContractFailed += value; }
            remove { ContractRuntimeHelper.ContractFailed -= value; }
        }

        /// <summary>
        /// Marker to indicate the end of the contract section of a method.
        /// </summary>
        [Conditional("NOT_SUPPORTED")]
        public static void EndContractBlock() { }

        /// <summary>
        /// Returns whether the <paramref name="predicate"/> returns <c>true</c> 
        /// for all elements in the <paramref name="collection"/>.
        /// </summary>
        /// <param name="collection">The collection from which elements will be drawn from to pass to <paramref name="predicate"/>.</param>
        /// <param name="predicate">Function that is evaluated on elements from <paramref name="collection"/>.</param>
        /// <returns><c>true</c> if and only if <paramref name="predicate"/> returns <c>true</c> for all elements in
        /// <paramref name="collection"/>.</returns>
        /// <seealso cref="System.Collections.Generic.List&lt;T&gt;.TrueForAll"/>
        [Pure]
        public static bool ForAll<T>(IEnumerable<T> collection, Predicate<T> predicate)
        {
#if CONTRACTS_LIGHT_QUANTIFIERS
            return CheckForAll(collection, predicate);
#endif

            return true;
        }

        private static bool CheckForAll<T>(IEnumerable<T> collection, Predicate<T> predicate)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }

            if (predicate == null)
            {
                throw new ArgumentNullException("predicate");
            }

            Contract.EndContractBlock();

            foreach (T t in collection)
            {
                if (!predicate(t))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Returns whether the <paramref name="predicate"/> returns <c>true</c> 
        /// for any integer starting from <paramref name="fromInclusive"/> to <paramref name="toExclusive"/> - 1.
        /// </summary>
        [Pure]
        public static bool Exists(int fromInclusive, int toExclusive, Predicate<int> predicate)
        {
#if CONTRACTS_LIGHT_QUANTIFIERS
            if (fromInclusive > toExclusive)
            {
                throw new ArgumentException("fromInclusive must be less than or equal to toExclusive.");
            }

            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            Contract.EndContractBlock();

            for (int i = fromInclusive; i < toExclusive; i++)
            {
                if (predicate(i))
                {
                    return true;
                }
            }

            return false;
#else
            return true;
#endif
        }

        /// <summary>
        /// Returns whether the <paramref name="predicate"/> returns <c>true</c> 
        /// for any element in the <paramref name="collection"/>.
        /// </summary>
        [Pure]
        public static bool Exists<T>(IEnumerable<T> collection, Predicate<T> predicate)
        {
#if CONTRACTS_LIGHT_QUANTIFIERS
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
            Contract.EndContractBlock();

            foreach (T t in collection)
            {
                if (predicate(t))
                {
                    return true;
                }
            }

            return false;
#else
            return true;
#endif
        }
    }
}