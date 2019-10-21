namespace System.Diagnostics.CodeAnalysis
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    internal class AssertsTrueAttribute : Attribute
    {
        public AssertsTrueAttribute() { }
    }
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    internal class AssertsFalseAttribute : Attribute
    {
        public AssertsFalseAttribute() { }
    }
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    internal class EnsuresNotNullAttribute : Attribute
    {
        public EnsuresNotNullAttribute() { }
    }

    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    internal class NotNullWhenAttribute : Attribute
    {
        public NotNullWhenAttribute(bool predicate) { }
    }

    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    internal class NotNullAttribute : Attribute
    {
        public NotNullAttribute() { }
    }

    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    internal class MaybeNullAttribute : Attribute
    {
        public MaybeNullAttribute() { }
    }
}