#if DEBUG
#define CONTRACTS_LIGHT_ASSERTS_DEBUG
#endif

#define CONTRACTS_LIGHT_ASSERTS
#define CONTRACTS_LIGHT_ASSERTS_FOR_ALL

using System;
using System.Diagnostics.ContractsLight;

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
                Contract.Check(s != null)?.Assert("custom message");
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
                Contract.CheckDebug(s != null)?.Assert("custom message");
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
                Contract.CheckDebug(s != null)?.Assert("custom message");
            }
        }
#endif
    }
}
