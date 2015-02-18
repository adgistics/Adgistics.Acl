using System;
using System.Collections.Generic;

namespace Modules.Acl
{
    /// <summary>
    ///   A 'user' type principal that can be used within the ACL module.
    /// </summary>
    public interface IUser : IPrincipal
    {
        /// <summary>
        ///   Gets the identifier of the user.
        /// </summary>
        Guid Identifier { get; }

        /// <summary>
        ///   Gets the names of the groups that the user is assigned to.
        /// </summary>
        IEnumerable<string> Groups { get; }
    }
}