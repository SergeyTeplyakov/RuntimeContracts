using System;
using System.Collections.Generic;
using System.Diagnostics.FluentContracts;
using System.Text;

#nullable enable
#pragma warning disable CS8604 // Possible null reference argument.

namespace RuntimeContracts.Test.FluentContracts
{
    public class Examples
    {
        public void NullChecks(string? v1, string? v2, string? v3)
        {
            Contract.Requires(v1 != null)?.IsTrue();

            check(v1); // no warning!

            check(v2); // warning, v2 may be null!

            Contract.Assert(v2 != null)?.IsTrue();
            check(v2); // no warning!

            Contract.Assert(!string.IsNullOrWhiteSpace(v3))?.IsTrue();
            check(v3); // still a warning, unfortunately!!

            if (!string.IsNullOrWhiteSpace(v3))
            {
                check(v3); // still a warning!
            }

            Contract.Assert(v3 != null && !string.IsNullOrWhiteSpace(v3))?.IsTrue();
            check(v3); // no warnings!!

            //// This
            //Contract.AssertNotNullOrEmpty(v3);
            //// Contract.Assert(!string.IsNullOrEmpty(v3))?.IsTrue();
            //// to ????
            //Contract.Assert(v3)?.NotNullOrWhiteSpace();

            void check(string s) { }
        }
    }
}

#pragma warning restore CS8604 // Possible null reference argument.