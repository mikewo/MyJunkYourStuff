using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;

namespace MyJunkYourStuff.Models
{
    public class AzureBlobImageRepository : IImageRepository
    {

        static readonly string _accountKey;
        static readonly string _accountName;
        static readonly string _containerName;

        private readonly CloudBlobContainer _container;

        /// <summary>
        /// Used to ensure the container and storage account is property initialized. As this is a static constructor it fires only
        /// the first time an instance of AzureBlobImageRepository is attempted to be created.
        /// Note that some might do this as part of a deployment script rather than leaving it for the code to do, but this
        /// works well for demos and when others may pull down the code to try it out.
        /// </summary>
        static AzureBlobImageRepository()
        {
            _accountKey = ConfigurationManager.AppSettings["Blob.storageAccountKey"];
            if (string.IsNullOrWhiteSpace(_accountKey))
            {
                throw new ConfigurationErrorsException("You must supply a Blob storage account key in the web.config under the appsettings key 'Blob.storageAccountKey'.");
            }

            _accountName = ConfigurationManager.AppSettings["Blob.storageAccountName"];
            if (string.IsNullOrWhiteSpace(_accountName))
            {
                throw new ConfigurationErrorsException("You must supply a Blob storage account name in the web.config under the appsettings key 'Blob.storageAccountName'.");
            }

            _containerName = ConfigurationManager.AppSettings["Blob.containerName"];
            if (string.IsNullOrWhiteSpace(_containerName))
            {
                throw new ConfigurationErrorsException("You must supply a Blob storage container name in the web.config under the appsettings key 'Blob.containerName'.");
            }

            try
            {
                //Verify the container exists.
                CloudBlobContainer container = getContainerReference();
                container.CreateIfNotExists();
                //Verify that the permissions are public read for Blobs.
                BlobContainerPermissions permissions = new BlobContainerPermissions() { PublicAccess = BlobContainerPublicAccessType.Blob };
                container.SetPermissions(permissions);     

            }
            catch (Exception ex)
            {
                throw new ConfigurationErrorsException("There was an exception attempting to verify the Blob Storage account is configured correctly. Make sure the key and account name are correctly set in the web.config.", ex);
            }
        }

        /// <summary>
        /// Initializes a new instance of the AzureBlobImageRepository class.
        /// </summary>
        public AzureBlobImageRepository()
        {
            _container = getContainerReference();
        }

        public string Add(string storeImageName, HttpPostedFileBase fileData)
        {
            CloudBlockBlob blob = _container.GetBlockBlobReference(storeImageName);

            blob.UploadFromStream(fileData.InputStream);

            blob.Metadata.Add("UploadedImageName", Path.GetFileName(fileData.FileName));
            blob.SetMetadata();
            blob.Properties.ContentType = fileData.ContentType;
            blob.SetProperties();

            return string.Concat(storeImageName);
        }

        public void Delete(string imagePath)
        {
            CloudBlockBlob blob = _container.GetBlockBlobReference(imagePath);
            blob.DeleteIfExists();
        }

        //Used to set up the reference to the container on the instance and during the static contstructor.
        private static CloudBlobContainer getContainerReference()
        {
            StorageCredentials creds = new StorageCredentials(_accountName, _accountKey);
            CloudStorageAccount account = new CloudStorageAccount(creds, true);

            CloudBlobClient client = account.CreateCloudBlobClient();

            CloudBlobContainer container = client.GetContainerReference(_containerName);

            return container;
        }
        
    }
}