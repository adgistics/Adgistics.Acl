namespace Modules.Acl.Example
{
    public sealed class RevisePrivilege : IPrivilege
    {
        private const string _identifier = "Revise";

        private const string _description =
            "Grants 'revise' privileges for resources.";

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