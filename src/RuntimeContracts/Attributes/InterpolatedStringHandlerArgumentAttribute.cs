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
        public string[] Arguments
        {
            get
            {
                throw null;
            }
        }

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
        }
    }
}