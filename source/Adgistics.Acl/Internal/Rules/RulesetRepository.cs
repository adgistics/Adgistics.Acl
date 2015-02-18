namespace Modules.Acl.Internal.Rules
{
    using System.IO;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Xml;

    using Utils;

    /// <summary>
    ///   A peristant repository for <see cref="Ruleset"/>s.
    /// </summary>
    internal sealed class RulesetRepository
    {
        #region Fields

        private static readonly DataContractSerializer Serializer;
        private static readonly XmlWriterSettings WriterSettings;

        private readonly FileInfo _rulesetFile;
        private readonly object _rulesetFileLocker;

        #endregion Fields

        #region Constructors

        static RulesetRepository()
        {
            // Serializer config
            Serializer = new DataContractSerializer(typeof(Ruleset));
            WriterSettings = new XmlWriterSettings() { Indent = true };
        }

        /// <summary>
        ///   Initializes a new instance of the 
        ///   <see cref="RulesetRepository"/> class.
        /// </summary>
        /// 
        /// <param name="api">The API.</param>
        public RulesetRepository(AccessControl api)
        {
            _rulesetFileLocker = new object();

            // Init base dir
            var rulesetDir = FileUtils.Concatenate(
                api.Config.RootDirectory,
                "Rulesets");

            if (false == rulesetDir.Exists)
            {
                rulesetDir.Create();
            }

            // Init ruleset file
            _rulesetFile = FileUtils.ConcatenateFile(
                rulesetDir,
                "RulesetData.xml");
        }

        #endregion Constructors

        #region Methods

        public Ruleset Load()
        {
            Ruleset result;

            lock (_rulesetFileLocker)
            {
                if (false == _rulesetFile.Exists)
                {
                    result = Ruleset.GetDefaultRuleSet();
                }
                else
                {
                    result = Deserialize(_rulesetFile);
                }
            }

            return result;
        }

        public void Save(Ruleset ruleset)
        {
            lock (_rulesetFileLocker)
            {
                Serialize(ruleset, _rulesetFile);
            }
        }

        private static Ruleset Deserialize(FileInfo source)
        {
            Ruleset result;

            using (var stream = source.OpenRead())
            {
                result = (Ruleset) Serializer.ReadObject(stream);
            }

            return result;
        }

        private static void Serialize(Ruleset ruleset, FileInfo target)
        {
            if (target.Exists)
            {
                // create a backup of existing version
                FileVersion.Create(target);

                target.Delete();
            }

            using (Stream stream = target.Create())
            using (XmlWriter writer = XmlWriter.Create(stream, WriterSettings))
            {
                Serializer.WriteObject(writer, ruleset);
            }
        }

        #endregion Methods
    }
}