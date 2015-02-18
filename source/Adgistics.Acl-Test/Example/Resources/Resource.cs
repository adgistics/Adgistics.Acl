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

using Modules.Acl.Core;

namespace Modules.Acl.Example
{
    internal sealed class Resource : IResource
    {
        public Resource(string identifier)
        {
            ResourceId = new ResourceId(
                "Modules.Acl.Example.Resource", identifier);
        }

        public ResourceId ResourceId { get; private set; }
    }
}