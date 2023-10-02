using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace System.Diagnostics.ContractsLight;

/// <summary>
/// Internal helper class responsible for raising appropriate exception type when a contract is violated.
/// </summary>
internal static class ContractRuntimeHelper
{
    // No inlining is explicit to put the method on the call stack.
    [MethodImpl(MethodImplOptions.NoInlining)]
    [DoesNotReturn]
    public static void ReportFailure(ContractFailureKind kind, string? msg, string? conditionTxt, Provenance provenance)
    {
        if (!RaiseContractFailedEvent(kind, msg, conditionTxt, provenance, out var text))
        {
            TriggerFailure(kind, text, msg, conditionTxt);
        }
#pragma warning disable CS8763 // A method marked [DoesNotReturn] should not return.
    }
#pragma warning restore CS8763

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ReportPreconditionFailure<TException>(string? msg, string? conditionTxt, Provenance provenance) where TException : Exception
#if !NETSTANDARD2_0
        // Previous version are relies on new constraint for exception construction.
        , new()
#endif
    {
        if (!RaiseContractFailedEvent(ContractFailureKind.Precondition, msg, conditionTxt, provenance, out var text))
        {
            // TODO: need to cache the factory or maybe switch to reflection-based approach.
            var factory = GenerateExceptionFactory<TException>();
            throw factory(text);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void TriggerFailure(ContractFailureKind kind, string? msg, string? userMessage, string? conditionTxt)
    {
        throw new ContractException(kind, msg, userMessage, conditionTxt);
    }

    // Reflection is only supported by NETSTANDARD2_0
#if NETSTANDARD2_0
        /// <summary>
        /// Helper that generates the following factory method:
        /// <code>
        /// public static TException CreateException(string arg) => new TException(arg);
        /// </code>
        /// </summary>
        /// <remarks>
        /// This method generates the following IL:
        /// <code>
        /// IL_0000: ldarg.0      // s
        /// IL_0001: newobj instance void[mscorlib] TException::.ctor(string)
        /// IL_0006: ret
        /// </code>
        /// </remarks>
        /// <exception cref="InvalidOperationException">Throw an exception if the <see cref="TException"/> does not have a constructor that takes a <code>string</code> argument. </exception>
        public static Func<string, TException> GenerateExceptionFactory<TException>()
        {
            var type = typeof(TException);
            
            var constructor = type.GetConstructor(new[] { typeof(string) });

            return arg => (TException)constructor.Invoke(new object[] { arg });
        }

#else //NETSTANDARD2_0
    public static Func<string, TException> GenerateExceptionFactory<TException>() where TException : new()
    {
        return arg => new TException();
    }
#endif

#if false
        /// <summary>
        /// Helper that generates the following factory method:
        /// <code>
        /// public static TException CreateException(string arg) => new TException(arg);
        /// </code>
        /// </summary>
        /// <remarks>
        /// This method generates the following IL:
        /// <code>
        /// IL_0000: ldarg.0      // s
        /// IL_0001: newobj instance void[mscorlib] TException::.ctor(string)
        /// IL_0006: ret
        /// </code>
        /// </remarks>
        /// <exception cref="InvalidOperationException">Throw an exception if the <see cref="TException"/> does not have a constructor that takes a <code>string</code> argument. </exception>
        public static Func<string, TException> GenerateExceptionFactory2<TException>()
        {
            var constructorInfo = typeof(TException).GetConstructor(new[] { typeof(string) });
            if (constructorInfo == null)
            {
                throw new InvalidOperationException(
                    $"Exception '{typeof(TException).Name}' does not have a constructor that takes a 'string' as an argument.");
            }

            var method = new DynamicMethod(
                name: "CreateException",
                returnType: typeof(TException),
                parameterTypes: new[] { typeof(string) },
                m: typeof(ContractRuntimeHelper).Module,
                skipVisibility: true);

            ILGenerator ilGen = method.GetILGenerator();

            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Newobj, constructorInfo);
            ilGen.Emit(OpCodes.Ret);
            return (Func<string, TException>)method.CreateDelegate(typeof(Func<string, TException>));
        }
#endif

    public static event EventHandler<ContractFailedEventArgs>? ContractFailed;

    /// <summary>
    /// Contract class call this method on a contract failure to allow listeners to be notified.
    /// The method should not perform any failure (assert/throw) itself.
    /// This method has 3 functions:
    /// 1. Call any contract hooks (such as listeners to Contract failed events)
    /// 2. Determine if the listeneres deem the failure as handled (then resultFailureMessage should be set to null)
    /// 3. Produce a localized resultFailureMessage used in advertising the failure subsequently.
    /// </summary>
    /// On exit: null if the event was handled and should not trigger a failure.
    ///          Otherwise, returns the localized failure message.
    [SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate")]
    [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
    public static bool RaiseContractFailedEvent(
        ContractFailureKind failureKind, 
        string? userMessage, 
        string? conditionText, 
        Provenance provenence, 
        out string resultFailureMessage)
    {
        bool handled = false;

        string displayMessage = "contract failed.";  // Incomplete, but in case of OOM during resource lookup...
        ContractFailedEventArgs? eventArgs = null;  // In case of OOM.
        try
        {
            displayMessage = GetDisplayMessage(failureKind, userMessage, conditionText, provenence);
            resultFailureMessage = displayMessage;

            EventHandler<ContractFailedEventArgs>? contractFailedEventLocal = ContractFailed;
            if (contractFailedEventLocal != null)
            {
                eventArgs = new ContractFailedEventArgs(failureKind, displayMessage, conditionText);
                foreach (EventHandler<ContractFailedEventArgs> handler in contractFailedEventLocal.GetInvocationList())
                {
                    try
                    {
                        handler(null, eventArgs);
                    }
                    catch (Exception e)
                    {
                        eventArgs.ThrownDuringHandler = e;
                        eventArgs.SetUnwind();
                    }
                }

                if (eventArgs.Unwind)
                {
                    // unwind
                    throw new ContractException(failureKind, displayMessage, userMessage, conditionText);
                }
            }
        }
        finally
        {
            if (eventArgs != null && eventArgs.Handled)
            {
                handled = true;
            }
        }

        return handled;
    }

    private static string GetDisplayMessage(ContractFailureKind failureKind, string? userMessage, string? conditionText, Provenance provenance)
    {
        string message = GetFailureMessage(failureKind, conditionText);

        return string.IsNullOrEmpty(userMessage)
            ? string.Format(CultureInfo.InvariantCulture, "{0}\r\n\tat {1}", message, provenance)
            : string.Format(CultureInfo.InvariantCulture, "{0}: {1}\r\n\tat {2}", message, userMessage, provenance);
    }

    private static string GetFailureMessage(ContractFailureKind failureKind, string? conditionText)
    {
        return string.IsNullOrEmpty(conditionText)
            ? string.Format(CultureInfo.InvariantCulture, "{0}", failureKind.ToDisplayString())
            : string.Format(CultureInfo.InvariantCulture, "{0} ({1})", failureKind.ToDisplayString(), conditionText);
    }
}