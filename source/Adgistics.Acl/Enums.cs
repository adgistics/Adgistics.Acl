// *****************************************************************************
// * Copyright (c) Adgistics Limited and others. All rights reserved.
// * The contents of this file are subject to the terms of the
// * Adgistics Development and Distribution License (the "License").
// * You may not use this file except in compliance with the License.
// *
// * http://www.adgistics.com/license.html
// *
// * See the License for the specific language governing permissions
// * and limitations under the License.
// *****************************************************************************
namespace Modules.Acl
{
    #region Enumerations

    internal enum Operation
    {
        Add = 1,
        Remove = 2
    }

    /// <summary>
    ///   ACL rule type.
    /// </summary>
    internal enum RuleType
    {
        /// <summary>
        ///   Will allow a user access.
        /// </summary>
        Allow = 1,

        /// <summary>
        ///   Will deny a user access.
        /// </summary>
        Deny = 2
    }

    #endregion Enumerations
}