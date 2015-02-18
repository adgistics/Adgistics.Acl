namespace Modules.Acl.Example
{
    public sealed class DeletePrivilege : IPrivilege
    {
        private const string _identifier = "Delete";

        private const string _description =
            "Grants 'delete' privileges for resources.";

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