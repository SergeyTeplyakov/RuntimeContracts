#if NETSTANDARD2_0
using System.Runtime.Serialization;
#endif

namespace System.Diagnostics.ContractsLight
{
#if NETSTANDARD2_0
    // Exception should be serializable only for desktop builds.
    // There is no domains in CoreCLR and the notion of exception serialization is not applicable there.
    [Serializable]
#endif
    internal sealed class ContractException : Exception
    {
#if NETSTANDARD2_0 // NETSTANDARD2_0
        [Serializable]
        private struct ContractExceptionData : ISafeSerializationData
#else
        private struct ContractExceptionData
#endif // NETSTANDARD2_0
        {
            public ContractFailureKind Kind;

            public string UserMessage;

            public string Condition;

#if NETSTANDARD2_0 // NETSTANDARD2_0
            void ISafeSerializationData.CompleteDeserialization(object obj)
            {
                ContractException ex = obj as ContractException;
                ex.m_data = this;
            }
#endif // NETSTANDARD2_0
        }

        private ContractExceptionData m_data = default(ContractExceptionData);

        public ContractFailureKind Kind => m_data.Kind;

        public string Failure => Message;

        public string UserMessage => m_data.UserMessage;

        public string Condition => m_data.Condition;

        public ContractException(ContractFailureKind kind, string failure, string userMessage, string condition, Exception innerException = null) : base(failure, innerException)
        {
            m_data.Kind = kind;
            m_data.UserMessage = userMessage;
            m_data.Condition = condition;
#if NETSTANDARD2_0
            SerializeObjectState += delegate (object exception, SafeSerializationEventArgs eventArgs)
            {
                eventArgs.AddSerializedState(m_data);
            };
#endif //NETSTANDARD2_0
        }
    }
}