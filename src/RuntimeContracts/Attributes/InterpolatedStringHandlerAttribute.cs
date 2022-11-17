﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if !NET6_0_OR_GREATER

namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    internal sealed class InterpolatedStringHandlerAttribute : Attribute
    {
        public InterpolatedStringHandlerAttribute()
        {
        }
    }
}

#endif