namespace Modules.Acl.Internal.Groups
{
    using System.IO;
    using System.Runtime.Serialization;
    using System.Xml;

    using Collections.Graphs;

    using Utils;

    internal sealed class GroupGraphRepository
    {
        #region Fields

        private static readonly DataContractSerializer Serializer;
        private static readonly XmlWriterSettings WriterSettings;

        private readonly AccessControl _api;
        private readonly FileInfo _groupGraphFile;
        private readonly object _groupsFileLocker;

        #endregion Fields

        #region Constructors

        static GroupGraphRepository()
        {
            // Serializer config
            Serializer = new DataContractSerializer(typeof(DirectedGraph<Group>));
            WriterSettings = new XmlWriterSettings() { Indent = true };
        }

        public GroupGraphRepository(
            AccessControl api)
        {
            _api = api;

            _groupsFileLocker = new object();

            // Init base dir
            var groupsDir = FileUtils.Concatenate(
                api.Config.RootDirectory,
                "Groups");

            if (false == groupsDir.Exists)
            {
                groupsDir.Create();
            }

            // Init groups data file
            _groupGraphFile = FileUtils.ConcatenateFile(
                groupsDir,
                "GroupGraphData.xml");
        }

        #endregion Constructors

        #region Methods

        public DirectedGraph<Group> Load()
        {
            DirectedGraph<Group> result;

            lock (_groupsFileLocker)
            {
                if (false == _groupGraphFile.Exists)
                {
                    result = new DirectedGraph<Group>();
                }
                else
                {
                    result = Deserialize(_groupGraphFile);
                }
            }

            return result;
        }

        public void Save(DirectedGraph<Group> groupData)
        {
            lock (_groupsFileLocker)
            {
                Serialize(groupData, _groupGraphFile);
            }
        }

        private static DirectedGraph<Group> Deserialize(FileInfo source)
        {
            DirectedGraph<Group> result;

            using (var stream = source.OpenRead())
            {
                result = (DirectedGraph<Group>)Serializer.ReadObject(stream);
            }

            return result;
        }

        private static void Serialize(DirectedGraph<Group> groupGraph, FileInfo target)
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
                Serializer.WriteObject(writer, groupGraph);
            }
        }

        #endregion Methods
    }
}