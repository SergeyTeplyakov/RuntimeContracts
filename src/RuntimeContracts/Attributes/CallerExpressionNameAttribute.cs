#if !NET6_0_OR_GREATER

namespace System.Runtime.CompilerServices
{
    //
    // Summary:
    //     Allows capturing of the expressions passed to a method.
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    internal sealed class CallerArgumentExpressionAttribute : Attribute
    {
        //
        // Summary:
        //     Gets the target parameter name of the CallerArgumentExpression.
        //
        // Returns:
        //     The name of the targeted parameter of the CallerArgumentExpression.
        public string ParameterName
        {
            get
            {
                throw null!;
            }
        }

        //
        // Summary:
        //     Initializes a new instance of the System.Runtime.CompilerServices.CallerArgumentExpressionAttribute
        //     class.
        //
        // Parameters:
        //   parameterName:
        //     The name of the targeted parameter.
        public CallerArgumentExpressionAttribute(string parameterName)
        {
        }
    }
}

#endif