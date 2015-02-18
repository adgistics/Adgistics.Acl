namespace Modules.Acl.Internal.Resources
{
    using System.Collections.Concurrent;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Xml;

    using Modules.Acl.Internal.Utils;

    internal sealed class ResourceRepository
    {
        #region Fields

        private static readonly DataContractSerializer Serializer;
        private static readonly XmlWriterSettings WriterSettings;

        private readonly AccessControl _api;
        private readonly FileInfo _resourcesFile;
        private readonly object _resourcesFileLocker;

        #endregion Fields

        #region Constructors

        static ResourceRepository()
        {
            // Serializer config
            Serializer =
                new DataContractSerializer(
                    typeof (
                        ConcurrentDictionary<ResourceId, ResourceRegistration>));
            WriterSettings = new XmlWriterSettings() { Indent = true };
        }

        public ResourceRepository(AccessControl api)
        {
            _api = api;

            _resourcesFileLocker = new object();

            // Init base dir
            var resourcesDir = FileUtils.Concatenate(
                api.Config.RootDirectory,
                "Resources");

            if (false == resourcesDir.Exists)
            {
                resourcesDir.Create();
            }

            // Init groups data file
            _resourcesFile = FileUtils.ConcatenateFile(
                resourcesDir,
                "ResourcesData.xml");
        }

        #endregion Constructors

        #region Methods

        public ConcurrentDictionary<ResourceId, ResourceRegistration> Load()
        {
            ConcurrentDictionary<ResourceId, ResourceRegistration> result;

            lock (_resourcesFileLocker)
            {
                if (false == _resourcesFile.Exists)
                {
                    result = new ConcurrentDictionary<ResourceId, ResourceRegistration>();
                }
                else
                {
                    result = Deserialize(_resourcesFile);
                }
            }

            return result;
        }

        public void Save(
            ConcurrentDictionary<ResourceId, ResourceRegistration> resourceData)
        {
            lock (_resourcesFileLocker)
            {
                Serialize(resourceData, _resourcesFile);
            }
        }

        private static ConcurrentDictionary<ResourceId, ResourceRegistration> Deserialize(
            FileInfo source)
        {
            ConcurrentDictionary<ResourceId, ResourceRegistration> result;

            using (var stream = source.OpenRead())
            {
                result = (ConcurrentDictionary<ResourceId, ResourceRegistration>)Serializer.ReadObject(stream);
            }

            return result;
        }

        private static void Serialize(
            ConcurrentDictionary<ResourceId, ResourceRegistration> resourceData,
            FileInfo target)
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
                Serializer.WriteObject(writer, resourceData);
            }
        }

        #endregion Methods
    }
}