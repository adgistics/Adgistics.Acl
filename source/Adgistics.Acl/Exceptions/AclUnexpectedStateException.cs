namespace Modules.Acl.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    ///   An error has occured due to the ACL data being in an unknown, 
    ///   unexpected, or corrupt.
    /// </summary>
    [Serializable]
    public sealed class AclUnexpectedStateException : ApplicationException
    {
        #region Constructors

        /// <summary>
        ///   Initializes a new instance of the 
        ///   <see cref="AclUnexpectedStateException"/> class.
        /// </summary>
        /// 
        /// <param name="message">
        ///   The error message that explains the reason for the exception.
        /// </param>
        /// 
        /// <param name="innerException">
        ///   The exception that is the cause of the current exception. If the 
        ///   <paramref name="innerException" /> parameter is not a null 
        ///   reference, the current exception is raised in a catch block that 
        ///   handles the inner exception.
        /// </param>
        public AclUnexpectedStateException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        ///   Initializes a new instance of the 
        ///   <see cref="AclUnexpectedStateException"/> class.
        /// </summary>
        /// 
        /// <param name="message">
        ///   A message that describes the error.
        /// </param>
        public AclUnexpectedStateException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///   Initializes a new instance of the 
        ///   <see cref="AclUnexpectedStateException"/> class.
        /// </summary>
        public AclUnexpectedStateException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AclUnexpectedStateException"/> class.
        /// </summary>
        /// <param name="info">The object that holds the serialized object data.</param>
        /// <param name="context">The contextual information about the source or destination.</param>
        private AclUnexpectedStateException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion Constructors
    }
}