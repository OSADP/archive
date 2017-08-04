//===============================================================================
// Microsoft patterns & practices
// Windows Azure Architecture Guide
//===============================================================================
// Copyright © Microsoft Corporation.  All rights reserved.
// This code released under the terms of the 
// Microsoft patterns & practices license (http://wag.codeplex.com/license)
//===============================================================================


using System;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.RetryPolicies;

namespace IDTO.Common.Storage
{
    public class FilesBlobContainer : IAzureBlobContainer<byte[]>
    {
        private readonly CloudStorageAccount account;
        private readonly CloudBlobContainer container;
        private readonly string contentType;

        public FilesBlobContainer(CloudStorageAccount account, string containerName, string contentType)
        {
            this.account = account;
            this.contentType = contentType;

            var client = this.account.CreateCloudBlobClient();
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(5), 3);
            client.RetryPolicy = linearRetryPolicy;

            this.container = client.GetContainerReference(containerName);
        }

        public void EnsureExist()
        {
            this.container.CreateIfNotExists();
            //this.container.SetPermissions(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });
        }

        public void Save(string objId, byte[] obj)
        {
            CloudBlockBlob blob = this.container.GetBlockBlobReference(objId);
            blob.Properties.ContentType = this.contentType;
            blob.UploadFromByteArray(obj, 0, obj.GetLength(0));
        }

        byte[] IAzureBlobContainer<byte[]>.Get(string objId)
        {
            CloudBlockBlob blob = this.container.GetBlockBlobReference(objId);
            try
            {
                byte[] downloadedBytes = new byte[0];
                blob.DownloadToByteArray(downloadedBytes, 0);
                return downloadedBytes;
            }
            catch (StorageException)
            {
                return null;
            }
        }

        public Uri GetUri(string objId)
        {
            CloudBlockBlob blob = this.container.GetBlockBlobReference(objId);
            return blob.Uri;
        }

        public void Delete(string objId)
        {
            CloudBlockBlob blob = this.container.GetBlockBlobReference(objId);
            blob.DeleteIfExists();
        }

        public void DeleteContainer()
        {
            this.container.Delete();
        }
    }
}