using System.Diagnostics.CodeAnalysis;

namespace System.Diagnostics.ContractsLight;

/// <summary>
/// Methods and classes marked with this attribute can be used within calls to Contract methods. Such methods not make any visible state changes.
/// </summary>
[Conditional("CONTRACTS_FULL")]
[AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Event | AttributeTargets.Delegate | AttributeTargets.Class | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class PureAttribute : Attribute
{
}

/// <summary>
/// Types marked with this attribute specify that a separate type contains the contracts for this type.
/// </summary>
[Conditional("CONTRACTS_FULL")]
[Conditional("DEBUG")]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Delegate, AllowMultiple = false, Inherited = false)]
// [Obsolete("Not supported by RuntimeContracts")]
public sealed class ContractClassAttribute : Attribute
{
    /// <nodoc />
    public ContractClassAttribute(Type typeContainingContracts)
    {
        TypeContainingContracts = typeContainingContracts;
    }

    /// <nodoc />
    public Type TypeContainingContracts { get; }
}

/// <summary>
/// Types marked with this attribute specify that they are a contract for the type that is the argument of the constructor.
/// </summary>
[Conditional("CONTRACTS_FULL")]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
// [Obsolete("Not supported by RuntimeContracts")]
public sealed class ContractClassForAttribute : Attribute
{
    /// <nodoc />
    public ContractClassForAttribute(Type typeContractsAreFor)
    {
        TypeContractsAreFor = typeContractsAreFor;
    }

    /// <nodoc />
    public Type TypeContractsAreFor { get; }
}

/// <summary>
/// This attribute is used to mark a method as being the invariant
/// method for a class. The method can have any name, but it must
/// return "void" and take no parameters. The body of the method
/// must consist solely of one or more calls to the method
/// Contract.Invariant. A suggested name for the method is 
/// "ObjectInvariant".
/// </summary>
[Conditional("CONTRACTS_FULL")]
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
// [Obsolete("Not supported by RuntimeContracts")]
public sealed class ContractInvariantMethodAttribute : Attribute
{
}

/// <summary>
/// Attribute that specifies that an assembly is a reference assembly with contracts.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly)]
// [Obsolete("Not supported by RuntimeContracts")]
public sealed class ContractReferenceAssemblyAttribute : Attribute
{
}

/// <summary>
/// Methods (and properties) marked with this attribute can be used within calls to Contract methods, but have no runtime behavior associated with them.
/// </summary>
[Conditional("CONTRACTS_FULL")]
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
// [Obsolete("Not supported by RuntimeContracts")]
public sealed class ContractRuntimeIgnoredAttribute : Attribute
{
}

/// <summary>
/// Instructs downstream tools whether to assume the correctness of this assembly, type or member without performing any verification or not.
/// Can use [ContractVerification(false)] to explicitly mark assembly, type or member as one to *not* have verification performed on it.
/// Most specific element found (member, type, then assembly) takes precedence.
/// (That is useful if downstream tools allow a user to decide which polarity is the default, unmarked case.)
/// </summary>
/// <remarks>
/// Apply this attribute to a type to apply to all members of the type, including nested types.
/// Apply this attribute to an assembly to apply to all types and members of the assembly.
/// Apply this attribute to a property to apply to both the getter and setter.
/// </remarks>
[Conditional("CONTRACTS_FULL")]
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Property)]
// [Obsolete("Not supported by RuntimeContracts")]
public sealed class ContractVerificationAttribute : Attribute
{
    /// <nodoc />
    public ContractVerificationAttribute(bool value) { Value = value; }

    /// <nodoc />
    public bool Value { get; }
}

/// <summary>
/// Allows a field f to be used in the method contracts for a method m when f has less visibility than m.
/// For instance, if the method is public, but the field is private.
/// </summary>
[Conditional("CONTRACTS_FULL")]
[AttributeUsage(AttributeTargets.Field)]
[SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments", Justification = "Thank you very much, but we like the names we've defined for the accessors")]
// [Obsolete("Not supported by RuntimeContracts")]
public sealed class ContractPublicPropertyNameAttribute : Attribute
{
    /// <nodoc />
    public ContractPublicPropertyNameAttribute(string name)
    {
        Name = name;
    }

    /// <nodoc />
    public string Name { get; }
}

/// <summary>
/// Enables factoring legacy if-then-throw into separate methods for reuse and full control over
/// thrown exception and arguments
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
[Conditional("CONTRACTS_FULL")]
// [Obsolete("Not supported by RuntimeContracts")]
public sealed class ContractArgumentValidatorAttribute : Attribute
{
}

/// <summary>
/// Enables writing abbreviations for contracts that get copied to other methods
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
[Conditional("CONTRACTS_FULL")]
// [Obsolete("Not supported by RuntimeContracts")]
public sealed class ContractAbbreviatorAttribute : Attribute
{
}

/// <summary>
/// Allows setting contract and tool options at assembly, type, or method granularity.
/// </summary>
[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
[Conditional("CONTRACTS_FULL")]
// [Obsolete("Not supported by RuntimeContracts")]
public sealed class ContractOptionAttribute : Attribute
{
    /// <nodoc />
    public ContractOptionAttribute(string category, string setting, bool enabled)
    {
        Category = category;
        Setting = setting;
        Enabled = enabled;
    }

    /// <nodoc />
    public ContractOptionAttribute(string category, string setting, string value)
    {
        Category = category;
        Setting = setting;
        Value = value;
    }

    /// <nodoc />
    public string Category { get; }

    /// <nodoc />
    public string Setting { get; }

    /// <nodoc />
    public bool Enabled { get; }

    /// <nodoc />
    public string? Value { get; }
}