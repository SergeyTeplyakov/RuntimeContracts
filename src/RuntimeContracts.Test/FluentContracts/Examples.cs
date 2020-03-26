using System;
using System.Collections.Generic;
using System.Diagnostics.ContractsLight;
using System.Text;

#nullable enable
#pragma warning disable CS8604 // Possible null reference argument.

namespace RuntimeContracts.Test.FluentContracts
{
    public class Examples
    {
        public void NullChecks(string? v1, string? v2, string? v3)
        {
            Contract.Check(v1 != null)?.Requires("msg");

            check(v1); // no warning!

            check(v2); // warning, v2 may be null!

            Contract.Check(v2 != null)?.Assert("msg");
            check(v2); // no warning!

            Contract.Check(!string.IsNullOrWhiteSpace(v3))?.Assert("msg");
            check(v3); // still a warning, unfortunately!!

            if (!string.IsNullOrWhiteSpace(v3))
            {
                check(v3); // still a warning!
            }

            Contract.Check(v3 != null && !string.IsNullOrWhiteSpace(v3))?.Assert("msg");
            check(v3); // no warnings!!

            //// This
            //Contract.AssertNotNullOrEmpty(v3);
            //// Contract.Assert(!string.IsNullOrEmpty(v3))?.IsTrue();
            //// to ????
            //Contract.Assert(v3)?.NotNullOrWhiteSpace();

            static void check(string s) { }
        }
    }
}

#pragma warning restore CS8604 // Possible null reference argument.