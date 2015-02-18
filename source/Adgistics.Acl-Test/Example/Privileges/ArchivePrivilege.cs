namespace Modules.Acl.Example
{
    public sealed class ArchivePrivilege : IPrivilege
    {
        private const string _identifier = "Archive";

        private const string _description =
            "Grants 'archive' privileges for resources.";

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