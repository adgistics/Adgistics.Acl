﻿namespace Modules.Acl.Example
{
    public sealed class UpdatePrivilege : IPrivilege
    {
        private const string _identifier = "Update";

        private const string _description =
            "Grants 'update' privileges for resources.";

        public string Identifier
        {
            get { return _identifier; }
        }

        public string Description
        {
            get { return _description; }
        }
    }
}