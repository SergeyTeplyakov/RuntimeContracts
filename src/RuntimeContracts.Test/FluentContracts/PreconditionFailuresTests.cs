#if DEBUG
#define CONTRACTS_LIGHT_PRECONDITIONS_DEBUG
#endif

#define CONTRACTS_LIGHT_PRECONDITIONS
#define CONTRACTS_LIGHT_PRECONDITIONS_FOR_ALL
using System;
using System.Diagnostics.FluentContracts;

using Xunit;

namespace RuntimeContracts.Test
{
    public class PreconditionFailuresTests
    {
        [Fact]
        public void PreconditionFailure()
        {
            Action a = () => WillFail(null);
            a.ShouldThrow();

            static void WillFail(string s)
            {
                Contract.Requires(s != null, "custom message")?.IsTrue();
            }
        }

        [Fact]
        public void PreconditionForAlFailure()
        {
            Action a = () => WillFail(new string[] { null });
            a.ShouldThrow();

            static void WillFail(string[] args)
            {
                Contract.RequiresForAll(args, s => s != null, "custom message")?.IsTrue();
            }
        }

#if DEBUG
        [Fact]
        public void PreconditionDebugFailure()
        {
            Action a = () => WillFail(null);
            a.ShouldThrow();

            static void WillFail(string s)
            {
                Contract.RequiresDebug(s != null, "custom message")?.IsTrue();
            }
        }
#else
        [Fact]
        public void PreconditionDebugShouldNotFailInRelease()
        {
            Action a = () => WillFail(null);
            a();

            static void WillFail(string s)
            {
                Contract.RequiresDebug(s != null, "custom message")?.IsTrue();
            }
        }
#endif
    }
}
