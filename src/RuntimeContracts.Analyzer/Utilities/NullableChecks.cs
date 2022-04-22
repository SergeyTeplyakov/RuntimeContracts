using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace RuntimeContracts.Analyzer.Utilities;

public static class NullableChecks
{
    [DebuggerStepThrough]
    public static T ThrowIfNull<T>(this T? value, [CallerArgumentExpression("value")] string paramName = "") where T : notnull
    {
        if (value is null)
        {
            throw new ArgumentNullException(paramName);
        }

        return value;
    }
}