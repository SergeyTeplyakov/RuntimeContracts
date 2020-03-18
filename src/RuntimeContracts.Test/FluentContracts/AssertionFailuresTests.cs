#if DEBUG
#define CONTRACTS_LIGHT_ASSERTS_DEBUG
#endif

#define CONTRACTS_LIGHT_ASSERTS
#define CONTRACTS_LIGHT_ASSERTS_FOR_ALL

using System;
using System.Diagnostics.FluentContracts;

using Xunit;

namespace RuntimeContracts.Test
{
    public class AssertionFailuresTests
    {
        [Fact]
        public void AssertionFailure()
        {
            Action a = () => WillFail(null);
            a.ShouldThrow();

            static void WillFail(string s)
            {
                Contract.Assert(s != null, "custom message")?.IsTrue();
            }
        }

        [Fact]
        public void AssertFailureAlwaysFail()
        {
            Action a = () => WillFail(null);
            a.ShouldThrow();

            static string WillFail(string s)
            {
                var exception = Contract.AssertFailure("custom message");
                return exception.Message;
            }
        }

        [Fact]
        public void AssertionForAlFailure()
        {
            Action a = () => WillFail(new string[] { null });
            a.ShouldThrow();

            static void WillFail(string[] args)
            {
                Contract.AssertForAll(args, s => s != null, "custom message")?.IsTrue();
            }
        }

#if DEBUG
        [Fact]
        public void AssertionDebugFailure()
        {
            Action a = () => WillFail(null);
            a.ShouldThrow();

            static void WillFail(string s)
            {
                Contract.AssertDebug(s != null, "custom message")?.IsTrue();
            }
        }
#else
        [Fact]
        public void AssertionDebugShouldNotFailInRelease()
        {
            Action a = () => WillFail(null);
            a();

            static void WillFail(string s)
            {
                Contract.AssertDebug(s != null, "custom message")?.IsTrue();
            }
        }
#endif
    }
}
