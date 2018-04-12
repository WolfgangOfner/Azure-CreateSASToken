using System;
using System.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;

namespace CreateSasToken
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);

            var token = CreateSasTokenForBlob(storageAccount);
            AccessBlobWithSasToken(token);

            token = CreateSasTokenForQueue(storageAccount);
            AccessQueueWithSasToken(token);

            token = CreateSasTokenForTable(storageAccount);
            AccessTableWithSasToken(token);
        }

        private static string CreateSasTokenForBlob(CloudStorageAccount storageAccount)
        {
            var blobClient = storageAccount.CreateCloudBlobClient();

            var sasPolicy =
                new SharedAccessBlobPolicy
                {
                    SharedAccessExpiryTime = DateTime.UtcNow.AddHours(1),
                    SharedAccessStartTime = DateTime.UtcNow.Subtract(new TimeSpan(0, 5, 0)),
                    Permissions = SharedAccessBlobPermissions.Read | SharedAccessBlobPermissions.Write |
                                  SharedAccessBlobPermissions.Delete | SharedAccessBlobPermissions.List
                };

            var files = blobClient.GetContainerReference("myblockcontainer");

            return files.GetSharedAccessSignature(sasPolicy);
        }

        private static void AccessBlobWithSasToken(string token)
        {
            var credentials = new StorageCredentials(token);
            var accountWithSas = new CloudStorageAccount(credentials, "wolfgangstorageaccount", null, true);
            var blobClient = accountWithSas.CreateCloudBlobClient();

            // do something with the container
            var container = blobClient.GetContainerReference("myblockcontainer");
        }

        private static string CreateSasTokenForQueue(CloudStorageAccount storageAccount)
        {
            var queueClient = storageAccount.CreateCloudQueueClient();
            var queue = queueClient.GetQueueReference("queue");
            var sasPolicy = new SharedAccessQueuePolicy
            {
                SharedAccessExpiryTime = DateTime.UtcNow.AddHours(1),
                Permissions = SharedAccessQueuePermissions.Read |
                              SharedAccessQueuePermissions.Add | SharedAccessQueuePermissions.Update |
                              SharedAccessQueuePermissions.ProcessMessages,
                SharedAccessStartTime = DateTime.UtcNow.Subtract(new TimeSpan(0, 5, 0))
            };

            return queue.GetSharedAccessSignature(sasPolicy);
        }

        private static void AccessQueueWithSasToken(string token)
        {
            var credentials = new StorageCredentials(token);
            var sasClient = new CloudQueueClient(new Uri("https://wolfgangstorageaccount.queue.core.windows.net/"), credentials);
            var sasQueue = sasClient.GetQueueReference("queue");

            sasQueue.AddMessage(new CloudQueueMessage("new SAS message"));
        }

        private static string CreateSasTokenForTable(CloudStorageAccount storageAccount)
        {
            var tableClient = storageAccount.CreateCloudTableClient();
            var table = tableClient.GetTableReference("orders");

            var sasPolicy =
                new SharedAccessTablePolicy
                {
                    SharedAccessExpiryTime = DateTime.UtcNow.AddHours(1),
                    Permissions = SharedAccessTablePermissions.Query |
                                  SharedAccessTablePermissions.Add | SharedAccessTablePermissions.Update |
                                  SharedAccessTablePermissions.Delete,
                    SharedAccessStartTime = DateTime.UtcNow.Subtract(new TimeSpan(0, 5, 0))
                };

            return table.GetSharedAccessSignature(sasPolicy);
        }

        private static void AccessTableWithSasToken(string token)
        {
            var credentials = new StorageCredentials(token);
            var accountWithSas = new CloudStorageAccount(credentials, "wolfgangstorageaccount", null, true);
            var tableClient = accountWithSas.CreateCloudTableClient();

            // do something with the table
            var table = tableClient.GetTableReference("orders");
        }
    }
}