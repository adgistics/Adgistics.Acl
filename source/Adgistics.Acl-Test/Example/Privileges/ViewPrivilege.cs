namespace Modules.Acl.Example
{
    public sealed class ViewPrivilege : IPrivilege
    {
        private const string _identifier = "View";

        private const string _description =
            "Grants 'view' privileges for resources.";

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