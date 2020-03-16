using System;
using static RuntimeContracts.Analyzer.Core.ContractMethodNames;
#nullable enable

namespace RuntimeContracts.Analyzer.Core
{
    [Flags]
    public enum ContractMethodNames
    {
        None,
        Assert = 1 << 0,
        Assume = 1 << 1,
        EndContractBlock = 1 << 2,
        Ensures = 1 << 3,
        EnsuresOnThrow = 1 << 4,
        Exists = 1 << 5,
        ForAll = 1 << 6,
        Invariant = 1 << 7,
        OldValue = 1 << 8,
        Requires = 1 << 9,
        Result = 1 << 10,
        ValueAtReturn = 1 << 11,
        
        RequiresNotNull = 1 << 12,
        AssertNotNull = 1 << 13,
        
        RequiresNotNullOrWhiteSpace = 1 << 14,
        AssertNotNullOrWhiteSpace = 1 << 15,
        
        RequiresNotNullOrEmpty = 1 << 16,
        AssertNotNullOrEmpty = 1 << 17,

        RequiresForAll = 1 << 18,
        RequiresDebug = 1 << 19,
        AssertForAll = 1 << 20,
        AssertDebug = 1 << 21,

        All = (1 << 22) - 1,
        AllAsserts = Assert | AssertNotNull | AssertNotNullOrEmpty | AssertNotNullOrWhiteSpace | AssertDebug | AssertForAll,
        AllRequires = Requires | RequiresNotNull | RequiresNotNullOrEmpty | RequiresNotNullOrWhiteSpace | RequiresDebug | RequiresForAll,
        AllFluentContracts = Requires | RequiresDebug | RequiresForAll | Assert | AssertDebug | AssertForAll,
    }

    public static class ContractMethodNamesExtensions
    {
        public static bool IsPrecondition(this ContractMethodNames contract)
            => (contract & AllRequires) != None;
        
        public static bool IsPostcondition(this ContractMethodNames contract)
            => (contract & (Ensures | EnsuresOnThrow)) != None;

        public static bool IsNullCheck(this ContractMethodNames contract)
            => (contract & (RequiresNotNull | AssertNotNull)) != None;

        public static bool IsNotNullOrEmpty(this ContractMethodNames contract)
            => (contract & (RequiresNotNullOrEmpty | AssertNotNullOrEmpty)) != None;

        public static bool IsNotNullOrWhiteSpace(this ContractMethodNames contract)
            => (contract & (RequiresNotNullOrWhiteSpace | AssertNotNullOrWhiteSpace)) != None;

        public static bool IsForAll(this ContractMethodNames contract)
            => (contract & RequiresForAll) != None;
    }
}
