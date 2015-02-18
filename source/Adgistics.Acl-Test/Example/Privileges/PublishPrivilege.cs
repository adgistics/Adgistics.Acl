namespace Modules.Acl.Example
{
    public sealed class PublishPrivilege : IPrivilege
    {
        private const string _identifier = "Publish";

        private const string _description =
            "Grants 'publish' privileges for resources.";

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