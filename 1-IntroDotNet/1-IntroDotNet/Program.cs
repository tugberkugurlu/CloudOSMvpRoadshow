using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Configuration;
using Microsoft.WindowsAzure.Storage.RetryPolicies;

namespace _1_IntroDotNet
{
    class Program
    {
        private static void Main(string[] args)
        {
            const string connectionString = "DefaultEndpointsProtocol=http;AccountName=[AccountName];AccountKey=[AccountKey]";
            const string connectionStringDevAccount = "UseDevelopmentStorage=true;DevelopmentStorageProxyUri=http://ipv4.fiddler";

            #region Intro

            // Intro(connectionStringDevAccount);
            // List(connectionStringDevAccount);
            // FolderBasedUpload(connectionStringDevAccount);
            // FolderBasedListing(connectionStringDevAccount);
            // SetMetadata(connectionStringDevAccount);

            #endregion

            #region Private Access

            // UploadAsPrivateBlob(connectionStringDevAccount);

            #endregion

            #region Shared Access Signature

            // GenerageSharedAccessSignature(connectionStringDevAccount);

            // string containerUriWithSas = GenerateReadWriteSas(connectionStringDevAccount);
            // UploadFileWithSas(containerUriWithSas);

            #endregion

            #region Large File Upload

            // UploadLargeFile(connectionStringDevAccount);

            #endregion

            #region Retry

            // RetryDefault(connectionStringDevAccount);
            // LinearRetrySample(connectionStringDevAccount);
            // NoRetrySample(connectionStringDevAccount);
            // NoRetryPerRequestSample(connectionStringDevAccount);

            #endregion

            #region Lease

            // AcquireLeaseSample(connectionStringDevAccount);

            #endregion

            Console.ReadLine();
        }

        static void Intro(string connectionString)
        {
            // Get the storage account reference
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);

            // Get the storage client reference
            CloudBlobClient client = storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a container
            CloudBlobContainer container = client.GetContainerReference("myfiles");

            // Create the container if not exists
            container.CreateIfNotExists(BlobContainerPublicAccessType.Blob);

            // Get blob reference
            string blobName = Guid.NewGuid().ToString() + ".gif";
            CloudBlockBlob blob = container.GetBlockBlobReference(blobName);
            blob.Properties.ContentType = "image/gif "; // set the Content-Type header
            blob.UploadFromFile(@"c:\dev\images\hamster.gif", FileMode.Open);
        }

        static void List(string connectionString)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudBlobClient client = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = client.GetContainerReference("myfiles");

            IEnumerable<IListBlobItem> blobs = container.ListBlobs(useFlatBlobListing: true).ToList();
            foreach (IListBlobItem blob in blobs)
            {
                Console.WriteLine(blob.Uri);
            }
        }

        static void FolderBasedUpload(string connectionString)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudBlobClient client = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = client.GetContainerReference("myfiles");

            string blobName = "animals/" + Guid.NewGuid().ToString() + ".gif";
            CloudBlockBlob blob = container.GetBlockBlobReference(blobName);
            blob.Properties.ContentType = "image/gif ";
            blob.UploadFromFile(@"c:\dev\images\hamster.gif", FileMode.Open);
        }

        static void FolderBasedListing(string connectionString)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudBlobClient client = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = client.GetContainerReference("myfiles");

            IEnumerable<IListBlobItem> blobs = container.ListBlobs(prefix: "animals/").ToList();
            foreach (IListBlobItem blob in blobs)
            {
                Console.WriteLine(blob.Uri);
            }
        }

        static void SetMetadata(string connectionString)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudBlobClient client = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = client.GetContainerReference("myfiles");

            string blobName = Guid.NewGuid().ToString() + ".gif";
            CloudBlockBlob blob = container.GetBlockBlobReference(blobName);
            blob.Properties.ContentType = "image/gif";
            blob.UploadFromFile(@"c:\dev\images\hamster.gif", FileMode.Open);
            blob.Metadata.Add("Title", "Foo Bar Lorem Ipsum");
            blob.SetMetadata();
        }

        static void UploadAsPrivateBlob(string connectionString)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudBlobClient client = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = client.GetContainerReference("privatefiles");
            container.CreateIfNotExists(BlobContainerPublicAccessType.Off);

            const string blobName = "hamster.gif";
            CloudBlockBlob blob = container.GetBlockBlobReference(blobName);
            blob.Properties.ContentType = "image/gif";
            blob.UploadFromFile(@"c:\dev\images\hamster.gif", FileMode.Open);
        }

        static void GenerageSharedAccessSignature(string connectionString)
        {
            const string blobName = "hamster.gif";
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudBlobClient client = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = client.GetContainerReference("privatefiles");
            ICloudBlob blob = container.GetBlobReferenceFromServer(blobName);
            string sas = blob.GetSharedAccessSignature(new SharedAccessBlobPolicy
            {
                Permissions = SharedAccessBlobPermissions.Read,
                SharedAccessStartTime = DateTimeOffset.UtcNow,
                SharedAccessExpiryTime = DateTimeOffset.UtcNow.AddMinutes(2)
            });

            Console.WriteLine(blob.Uri.AbsoluteUri + sas);
        }

        static string GenerateReadWriteSas(string connectionString)
        {
            const string blobName = "hamster.gif";
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudBlobClient client = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = client.GetContainerReference("animals");
            container.CreateIfNotExists(BlobContainerPublicAccessType.Off);

            return container.Uri.AbsoluteUri + container.GetSharedAccessSignature(new SharedAccessBlobPolicy
            {
                Permissions = SharedAccessBlobPermissions.Write | SharedAccessBlobPermissions.Read,
                SharedAccessStartTime = DateTimeOffset.UtcNow,
                SharedAccessExpiryTime = DateTimeOffset.UtcNow.AddMinutes(2)
            });
        }

        static void UploadFileWithSas(string containerUriWithSas)
        {
            const string blobName = "hamster.gif";
            CloudBlobContainer container = new CloudBlobContainer(new Uri(containerUriWithSas));
            CloudBlockBlob blob = container.GetBlockBlobReference(blobName);
            blob.Properties.ContentType = "image/gif";
            blob.UploadFromFile(@"c:\dev\images\hamster.gif", FileMode.Open);
        }

        static void UploadLargeFile(string connectionString)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudBlobClient client = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = client.GetContainerReference("myfiles");

            string blobName = Guid.NewGuid().ToString() + ".zip";
            CloudBlockBlob blob = container.GetBlockBlobReference(blobName);
            blob.Properties.ContentType = "application/octet-stream";
            blob.UploadFromFile(@"c:\dev\files\large-file.zip", FileMode.Open, options: new BlobRequestOptions
            {
                ParallelOperationThreadCount = Environment.ProcessorCount * 2,
                ServerTimeout = TimeSpan.FromMinutes(2)
            });
        }

        static void RetryDefault(string connectionString)
        {
            // By default, Exponential Retry Policy
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudBlobClient client = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = client.GetContainerReference("myfiles");

            IEnumerable<IListBlobItem> blobs = container.ListBlobs(useFlatBlobListing: true).ToList();
            foreach (IListBlobItem blob in blobs)
            {
                Console.WriteLine(blob.Uri);
            }
        }

        static void LinearRetrySample(string connectionString)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudBlobClient client = storageAccount.CreateCloudBlobClient();
            IRetryPolicy linearRetryPolicy = new LinearRetry(deltaBackoff: TimeSpan.FromSeconds(2), maxAttempts: 10);
            client.RetryPolicy = linearRetryPolicy;
            CloudBlobContainer container = client.GetContainerReference("myfiles");
            IEnumerable<IListBlobItem> blobs = container.ListBlobs(useFlatBlobListing: true).ToList();
            foreach (IListBlobItem blob in blobs)
            {
                Console.WriteLine(blob.Uri);
            }
        }

        static void NoRetrySample(string connectionString)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudBlobClient client = storageAccount.CreateCloudBlobClient();
            client.RetryPolicy = new NoRetry();
            CloudBlobContainer container = client.GetContainerReference("myfiles");
            IEnumerable<IListBlobItem> blobs = container.ListBlobs(useFlatBlobListing: true).ToList();
            foreach (IListBlobItem blob in blobs)
            {
                Console.WriteLine(blob.Uri);
            }
        }

        static void NoRetryPerRequestSample(string connectionString)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudBlobClient client = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = client.GetContainerReference("myfiles");
            IEnumerable<IListBlobItem> blobs = container.ListBlobs(useFlatBlobListing: true, options: new BlobRequestOptions
            {
                RetryPolicy = new NoRetry()
            }).ToList();

            foreach (IListBlobItem blob in blobs)
            {
                Console.WriteLine(blob.Uri);
            }
        }

        static void AcquireLeaseSample(string connectionString)
        {
            const string blobName = "rating.txt";
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudBlobClient client = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = client.GetContainerReference("myfiles");
            CloudBlockBlob blob = container.GetBlockBlobReference(blobName);
            blob.UploadText("0", Encoding.UTF8);

            Parallel.For(0, 20, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount * 2 }, i =>
            {
                CloudStorageAccount sAccount = CloudStorageAccount.Parse(connectionString);
                CloudBlobClient bClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer bContainer = client.GetContainerReference("myfiles");
                CloudBlockBlob blobRef = container.GetBlockBlobReference(blobName);

                bool isOk = false;
                while (isOk == false)
                {
                    try
                    {
                        // The Lease Blob operation establishes and manages a lock on a blob for write and delete operations. 
                        // The lock duration can be 15 to 60 seconds, or can be infinite.
                        string leaseId = blobRef.AcquireLease(TimeSpan.FromSeconds(15), Guid.NewGuid().ToString());
                        using (Stream stream = blobRef.OpenRead())
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            int raitingCount = int.Parse(reader.ReadToEnd());
                            byte[] bytesToUpload = Encoding.UTF8.GetBytes((raitingCount + 1).ToString(CultureInfo.InvariantCulture));
                            blobRef.UploadFromByteArray(bytesToUpload, 0, bytesToUpload.Length, AccessCondition.GenerateLeaseCondition(leaseId));
                        }

                        blobRef.BreakLease(breakPeriod: TimeSpan.Zero, accessCondition: AccessCondition.GenerateLeaseCondition(leaseId));
                        isOk = true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            });
        }
    }
}