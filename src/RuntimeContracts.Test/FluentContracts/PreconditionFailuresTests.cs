#if DEBUG
#define CONTRACTS_LIGHT_PRECONDITIONS_DEBUG
#endif

#define CONTRACTS_LIGHT_PRECONDITIONS
#define CONTRACTS_LIGHT_PRECONDITIONS_FOR_ALL
using System;
using System.Diagnostics.ContractsLight;

using Xunit;

namespace RuntimeContracts.Test;

public class PreconditionFailuresTests
{
    [Fact]
    public void PreconditionFailure()
    {
        Action a = () => WillFail(null);
        a.ShouldThrow();

        static void WillFail(string s)
        {
            Contract.Check(s != null)?.Requires("custom message");
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
            Contract.CheckDebug(s != null)?.Requires("custom message");
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
                Contract.CheckDebug(s != null)?.Requires("custom message");
            }
        }
#endif
}