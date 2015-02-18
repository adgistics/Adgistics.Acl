namespace Modules.Acl.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    // TODO: Add logging message for the privilege in question.
    /// <summary>
    ///   An error has occured due to an action against a privilege which has
    ///   not been registered for usage in the ACL system.
    /// </summary>
    [Serializable]
    public sealed class PrivilegeNotRegisteredException : ApplicationException
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
        public PrivilegeNotRegisteredException(string message, Exception innerException)
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
        public PrivilegeNotRegisteredException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///   Initializes a new instance of the 
        ///   <see cref="AclUnexpectedStateException"/> class.
        /// </summary>
        public PrivilegeNotRegisteredException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AclUnexpectedStateException"/> class.
        /// </summary>
        /// <param name="info">The object that holds the serialized object data.</param>
        /// <param name="context">The contextual information about the source or destination.</param>
        private PrivilegeNotRegisteredException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion Constructors
    }
}