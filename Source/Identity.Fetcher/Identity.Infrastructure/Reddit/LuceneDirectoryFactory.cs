using System;
using System.IO;
using Identity.Domain.RedditIndexes;
using Lucene.Net.Store;
using Lucene.Net.Store.Azure;
using Microsoft.WindowsAzure.Storage;

namespace Identity.Infrastructure.Services
{
    public class LuceneDirectoryFactory
    {
        private readonly CloudStorageAccount azureStorageAccount;
        private readonly string localStoragePath;

        public LuceneDirectoryFactory(CloudStorageAccount azureStorageAccount, string localStoragePath)
        {
            this.azureStorageAccount = azureStorageAccount;
            this.localStoragePath = localStoragePath;
        }

        public Lucene.Net.Store.Directory Create(long id, IndexStorageLocation storageLocation)
        {
            switch (storageLocation)
            {
                case IndexStorageLocation.Local:
                    return FSDirectory.Open(Path.Combine(localStoragePath, id.ToString()));
                case IndexStorageLocation.Azure:
                    return new AzureDirectory(azureStorageAccount, "RedditIndex"); //TODO: support for multiple Azure indexes....
                default:
                    throw new ArgumentOutOfRangeException(nameof(storageLocation), storageLocation, null);
            }
        }
    }
}