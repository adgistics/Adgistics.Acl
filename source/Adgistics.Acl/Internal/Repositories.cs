namespace Modules.Acl.Internal
{
    using Modules.Acl.Internal.Groups;
    using Modules.Acl.Internal.Resources;

    using Rules;

    /// <summary>
    /// 
    /// </summary>
    internal sealed class Repositories
    {
        #region Fields

        private readonly AccessControl _api;
        private readonly GroupGraphRepository _groupGraphRepository;
        private readonly RulesetRepository _rulesetRepository;

        private ResourceRepository _resourcesRepository;

        #endregion Fields

        #region Constructors

        public Repositories(AccessControl api)
        {
            _api = api;

            _groupGraphRepository = new GroupGraphRepository(_api);

            _rulesetRepository = new RulesetRepository(_api);

            _resourcesRepository = new ResourceRepository(_api);
        }

        #endregion Constructors

        #region Properties

        public GroupGraphRepository GroupGraphRepository
        {
            get { return _groupGraphRepository; }
        }

        public ResourceRepository ResourceRepository
        {
            get { return _resourcesRepository; }
        }

        public RulesetRepository RulesetRepository
        {
            get { return _rulesetRepository; }
        }

        #endregion Properties
    }
}