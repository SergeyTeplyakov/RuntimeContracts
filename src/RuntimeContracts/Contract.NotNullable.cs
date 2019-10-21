// --------------------------------------------------------------------
// 
// Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// --------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;

#nullable enable

namespace System.Diagnostics.ContractsLight
{
    public partial class Contract
    {
        private class UnreachableException : Exception { }

        public static void RequiresNotNull<T>([NotNull]T? o, string? userMessage = null) where T : class
        {
            if (o == null)
            {
                Requires(false, userMessage ?? "The value should not be null.");
                throw new UnreachableException();
            }
        }

        public static void RequiresNotNull<T>([NotNull]T? o, string? userMessage = null) where T : struct
        {
            if (o == null)
            {
                Requires(false, userMessage ?? "The value should not be null.");
                throw new UnreachableException();
            }
        }

        public static void RequiresNotNullOrEmpty([NotNull]string? o, string? userMessage = null)
        {
            if (string.IsNullOrEmpty(o))
            {
                Requires(false, userMessage ?? "The value should not be null or empty.");
                throw new UnreachableException();
            }
        }

        public static void RequiresNotNullOrWhiteSpace([NotNull]string? o, string? userMessage = null)
        {
            if (string.IsNullOrWhiteSpace(o))
            {
                Requires(false, userMessage ?? "The value should not be null or empty.");
                throw new UnreachableException();
            }
        }

        public static T AssertNotNull<T>([NotNull]T? value, string? userMessage = null) where T : class
        {
            if (value == null)
            {
                Assert(false, userMessage ?? "The value should not be null.");
                throw new UnreachableException();
            }

            return value;
        }

        public static string AssertNotNullOrEmpty([NotNull]string? o, string? userMessage = null)
        {
            if (string.IsNullOrEmpty(o))
            {
                Assert(false, userMessage ?? "The value should not be null or empty.");
                throw new UnreachableException();
            }

            return o!;
        }

        public static string AssertNotNullOrWhiteSpace([NotNull]string? o, string? userMessage = null)
        {
            if (string.IsNullOrWhiteSpace(o))
            {
                Assert(false, userMessage ?? "The value should not be null or empty.");
                throw new UnreachableException();
            }

            return o!;
        }

        public static T AssertNotNull<T>([NotNull]T? value, string? userMessage = null) where T : struct
        {
            if (value == null)
            {
                Assert(false, userMessage ?? "The value should not be null.");
                throw new UnreachableException();
            }

            return value.Value;
        }
    }
}