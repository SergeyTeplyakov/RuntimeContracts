// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if !NET6_0_OR_GREATER

namespace System.Runtime.CompilerServices
{
    //
    // Summary:
    //     Indicates which arguments to a method involving an interpolated string handler
    //     should be passed to that handler.
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    internal sealed class InterpolatedStringHandlerArgumentAttribute : Attribute
    {
        //
        // Summary:
        //     Gets the names of the arguments that should be passed to the handler.
        public string[] Arguments { get; }

        //
        // Summary:
        //     Initializes a new instance of the System.Runtime.CompilerServices.InterpolatedStringHandlerArgumentAttribute
        //     class.
        //
        // Parameters:
        //   argument:
        //     The name of the argument that should be passed to the handler.
        public InterpolatedStringHandlerArgumentAttribute(string argument)
        {
            Arguments = new string[] { argument };
        }

        //
        // Summary:
        //     Initializes a new instance of the System.Runtime.CompilerServices.InterpolatedStringHandlerArgumentAttribute
        //     class.
        //
        // Parameters:
        //   arguments:
        //     The names of the arguments that should be passed to the handler.
        public InterpolatedStringHandlerArgumentAttribute(params string[] arguments)
        {
            Arguments = arguments;
        }
    }
}

#endif