namespace Modules.Acl.Example
{
    public sealed class AdminPrivilege : IPrivilege
    {
        private const string _identifier = "Admin";

        private const string _description =
            "Grants 'admin' privileges for resources.";

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