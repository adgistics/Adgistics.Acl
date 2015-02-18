namespace Modules.Acl.Internal
{
    internal sealed class RuleAssertCache
    {
        private readonly AccessControl _api;

        public RuleAssertCache(AccessControl api)
        {
            _api = api;
        }
    }
}