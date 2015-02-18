namespace Modules.Acl.Example
{
    public sealed class SubmitPrivilege : IPrivilege
    {
        private const string _identifier = "Submit";

        private const string _description =
            "Grants 'submit' privileges for resources.";

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