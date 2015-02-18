namespace Modules.Acl.Example
{
    public sealed class EditPrivilege : IPrivilege
    {
        private const string _identifier = "Edit";

        private const string _description =
            "Grants 'edit' privileges for resources.";

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