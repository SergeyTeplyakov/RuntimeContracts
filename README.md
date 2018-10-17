# RuntimeContracts
Library-based implementation of Code Contracts-like API.

# Overview

Design by contract (DbC) is a great paradigm that enforces developers to think about object interaction in terms of their responsibilities: what a "client" should provided to a "server" in order to get the results? I.e. what the preconditions are? And what the "server" will provide if preconditions are met? What the invariants and postconditions?

I found that this way of thinking helps me to separate the responsibilities across the object graph and clearly defined what should be provided, what will be achieved, what can I assume and not.

## Key aspects of DbC

Describing program's behavior in terms of assertions opens some interesting possibilities.

* **Compile time and runtime verifications.** Expressiveness of assertions makes it possible to build tools to validate correctness of a program at compile time and leave a subset of the assertions at runtime.
* **Fine-grained control in runtime.** You can enable/disable one type of assertions while keeping the other. For instance, in debug mode all the assertions should be on, but in release mode an application author may decide to keep preconditions only and remove all the other checks.
* **Runnable documentation.** Contracts in the code can be treated as a executable documentation that augment the type system.

# Design by Contract in .NET
Contract-based programming in .NET is represented via [`Contract`](https://referencesource.microsoft.com/#mscorlib/system/diagnostics/contracts/contracts.cs,c575dbe300e57438) class. This class was added in the BCL in version 4 to mscorlib but it requires external tools for compile-time and runtime verifications.

This design decision allowed gradual adoption of the DbC but faced some serious challenges along the way.

The main challenge is the necessaty of external tools in order to "embed" assertions in the final code. This was a natural requirement, because some assertions (like postconditions and invariants) require some code manipulation at compile time and there was no other ways to achieve this without changing all .NET languages. For instance, the postonciditons are checked at every exit point of a method, and library-based approach means that the IL code must be rewritten in order to embed them and have any impact at runtime.

There are few critical issues with the existing tooling. Current behavior of both static checker and IL rewriter is not deterministic, the tools do not support .NET core and, most importantly, they're [open sourced](https://github.com/Microsoft/CodeContracts) but no longer supported.

# Runtime Contracts
Unlike Code Contracts, "Runtime Contracts" library is light-weight, library-only (no tools required) version that provides limited but useful functionality.

All the assertions are defined in [`System.Diagnostics.ContractsLight.Contract`](https://github.com/SergeyTeplyakov/RuntimeContracts/blob/master/src/RuntimeContracts/Contract.cs#L29) class. The namespace name is intentionaly close to Code Contracts version, because it helps the migration process from Code Contracts to Runtime Contracts just by simple find and replace of `using` statements in the code. 

All the members in the new `Contract` class are conditional and in order to use them you should defined a symbol, like `CONTRACTS_LIGHT_PRECONDITIONS` to enable preconditions, and `CONTRACTS_LIGHT_ASSERTS` to enable `Contract.Assert` and `Contract.Assume` methods.

## Known limitations
Library-based approach implies some serious limitations.

### Preconditions for constructors
```csharp
public class Base
{
    private readonly int _length;
    public Base(int length) => _length = length;
}

public class Derived : Base
{
    public Derived(string str): base(str.Length)
    {
        Contract.Requires(str != null);
    }
}
```

Code Contracts IL rewriter recognizes this pattern and moves the precondition check before a base constructor call. Runtime Contracts is just a library that does not affect how program runs. I.e. `new Derived(null)` will cause `NullReferenceException` in case of Runtime Contracts, but will fail with `ContractException` in case of Code Contracts.

### Preconditions for async methods and iterator blocks
```csharp
public static async Task StartTask(string arg)
{
    Contract.Requires(arg != null);
}

public static IEnumerable<int> Range(int count)
{
    Contract.Requires(count >= 0);
    return Enumerable.Range(1, count);
}

var task = StartTask(null); // 1
task.GetAwaiter().GetResult(); // 2

var sequence = Range(-1); // 3
var list = sequence.ToList(); // 4
```

A behavior of iterator blocks and async methods is different from regular methods. Code Contracts IL rewriter is able to recognize them and change their behavior in terms of preconditions.
For instance, running the code above with Code Contracts will cause contract violations at lines (1) and (3), but Runtime Contracts will fail at lines (2) and (4).

### No inherited contracts
It is impossible to defined contracts for interfaces and abstract classes using Runtime Contracts. This feature requires some form of code manipulation.

### Unsupported features
Some features are not possible without IL (or source) manipulation at compile time:
* The following assertions are not supported: `Contract.Ensures`, `Contract.EnsuresOnThrow`, `Contract.Invariant`.
* Interface contracts and contracts for abstract classes are not supported as well.

# RuntimeContracts.Analyzer
Runtime Contracts accompanied with a Roslyn-based analyzer that helps preventing some common issues. Right now there is only one rule there that will warn you if a project references Runtime Contracts but still uses `Contract` class from the BCL.

# The simplest way to enable Runtime Contracts with SDK projects
If you use SDK projects then the simplest way to enable Runtime Contracts for the entire source tree is by using `Directory.Build.targets` file. Just create one in the root of your codebase with the following content:

```
<Project DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
<PropertyGroup>
  <DefineConstants>$(DefineConstants);CONTRACTS_LIGHT_PRECONDITIONS;CONTRACTS_LIGHT_ASSERTS</DefineConstants>
</PropertyGroup>
</Project>
```

You can keep only preconditions or only assertions there, or you can redefined the behavior by creating another `Directory.Build.targets` in subfolders.