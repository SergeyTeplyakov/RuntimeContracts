using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.ContractsLight;
using System.Runtime.CompilerServices;
using System.Text;

#nullable enable

namespace System.Diagnostics.FluentContracts
{
    /// <summary>
    /// Contains static methods for representing program contracts such as preconditions and assertions using allocation-free
    /// fluent API-based syntax.
    /// </summary>
    /// <remarks>
    /// Unlike <see cref="System.Diagnostics.Contracts.Contract"/> the following class does not require any tools in the build pipeline.
    /// A user may control the "level" by defining following symbols:
    /// - CONTRACTS_LIGHT_PRECONDITIONS - to enable preconditions.
    /// - CONTRACTS_LIGHT_ASSERTS to enable <see cref="Assert"/> and <see cref="Assume"/> methods.
    ///
    /// Failed precondition/assertion triggers <see cref="ContractFailed"/> event and if the handler of that event will not "handle" the violation,
    /// <see cref="ContractException"/> will be thrown.
    ///
    /// Every assertion in this type takes two additional optional arguments: 'path' and 'lineNumber'.
    /// Unfortunately as of Sep 2017 csc can mess up with line numbers for async methods in release mode.
    /// To get the correct line and column information we need to use <see cref="CallerFilePathAttribute"/> and <see cref="CallerLineNumberAttribute"/> attributes.
    /// 
    /// Unlike <see cref="ContractsLight.Contract"/>, the API provided by this does not include a user-defined message
    /// and methods like <see cref="Requires(bool, string, int)"/> won't fail by themselves.
    /// They rather return a struct instance that should be validated by calling one of the extension
    /// methods, like <see cref="ContractFluentExtensions.IsTrue(in PreconditionFailure, string)"/> to actually
    /// trigger the validation.
    /// 
    /// This separation of concerns is very intentional and designed to avoid message allocation
    /// when the predicate is false.
    /// 
    /// Here are two examples:
    /// <code>
    /// // An old style
    /// Contract.Requires(x > 5, $"x > 5, x=[{x}]");
    /// // A new style
    /// Contract.Requires(x > 5)?.IsTrue($"x > 5, x=[{x}]");
    /// </code>
    /// The first code will allocate a string every time the contract is checked. I.e. the allocation will happened
    /// for successful case as well.
    /// But the second call won't allocation if the contract is not allocated.
    /// </remarks>
    public static class Contract
    {
        /// <inheritdoc cref="ContractsLight.Contract.Requires(bool, string, string, int)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PreconditionFailure? Requires(
            [DoesNotReturnIf(false)]
            bool condition,
            [CallerFilePath] string path = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            if (!condition)
            {
                return new PreconditionFailure(path, lineNumber);
            }

            return null;
        }

        /// <inheritdoc cref="ContractsLight.Contract.Requires(bool, string, string, int)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AssertionFailure? Assert(
            [DoesNotReturnIf(false)]
            bool condition,
            [CallerFilePath] string path = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            if (!condition)
            {
                return new AssertionFailure(path, lineNumber);
            }

            return null;
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
    }
}
