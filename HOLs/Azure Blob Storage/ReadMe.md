# Adding Azure Blob Storage #

In this Hands on Lab (HOL) you will add using Azure Blob Storage to a simple web site.  We will upload files and delete files from this persistent storage, as well as serve up content from Blob Storage rather than taking up processing on our web server.  

This lab assumes that while you may not have experience with Microsoft Azure, you do have some familiarity with Visual Studio and C#.

**Estimated Time: 15 minutes**

## Prerequisites ##
In order to complete this lab you will need the following:

- Visual Studio 2013 (with Update 4 or higher)
	- 90 Day trial from http://www.visualstudio.com/en-us/downloads/download-visual-studio-vs.aspx
	- Alternative: Community Edition of Visual Studio 2014 with Update 4 at http://www.visualstudio.com/en-us/downloads/download-visual-studio-vs.aspx
	- It is assumed IIS Express is installed and used in this HOL.
- Azure SDK (2.5 or higher) 
	- Obtain from http://azure.microsoft.com/en-us/downloads/ or Install from the [Microsoft Web Platform Installer](http://www.microsoft.com/web/downloads/platform.aspx "Web Platform Installer"). 
- An Azure Subscription 
	- Free Month Trial: sign up via http://azure.microsoft.com/en-us/pricing/free-trial/ 
	- OR MSDN Benefits: http://azure.microsoft.com/en-us/pricing/member-offers/msdn-benefits/ 
	- OR Use an existing subscription
- The source for the web application
	- You can use Git to fork/clone our repository at https://github.com/mikewo/MyJunkYourStuff
	- OR You can simply download the code as a Zip at the same address if you aren't familiar with Git.


### Disclaimer
This HOL features a simple MVC 5 application created with Visual Studio.  Note there are many aspects of this site that are NOT production worthy.  Security, for example, is nonexistent.  **We do NOT recommend taking this site as an example of how to write web sites**.  The purpose of this site is to show of very specific aspects of hosting a site on Azure and using Azure services.  There are many resources out on the internet that teach the best ways to handle security, ASP.NET MVC, namespacing, data validation, etc.  We highly encourage you to read up on those, or attend other presentations/conferences around those topics to learn more.


The HOL also walks through using the Azure Preview Portal for some of the instructions.  Note that the Azure Preview Portal changes quite regularly with new updates and functionality.  Depending on when you are using these notes the instructions below may not match what you are seeing online.

## Creating an Azure Storage Account
Before we can store anything on Azure Blob Storage we need to create a Storage Account and retrieve the access keys so that the website will have the ability to write new location images. 

1. Open a browser and navigate to the Azure Preview Portal: https://portal.azure.com 
>Note that you can accomplish this HOL using the current *Full* Azure Portal as well, but the steps would be slightly different.
2. Log in to the Portal using credentials that have access to an Azure subscription.
3. On the bottom left of the screen click the big **New** button.  
4. Select ***Everything*** from the top of the list that appears.
5. Select **Storage, cache, + backup** from the categories.
6. Select **Storage** by Microsoft, usually found under the "Recommended" area.
7. Click **Create** on the blade describing the storage offering.
8. Type a name for the storage account.  This name must be unique globally as it will become the URL for your storage account.  A green checkmark will appear if the name is acceptable.
9. Leave the pricing tier at **Standard-GRS**.
10. Note the resource group is created for you or you can select an already created one.
11. Verify you are creating this for the subscription you intend.
12. Set the location you wish the storage account to reside in.  You will want this to be the same location you deployed the website to.
13. Leave the diagnostics **Not Configured**.  You can turn this on if you'd like, but it isn't necessary for the HOL.  See the Additional Resources section for more information on the Azure Storage Diagnostics.
14. Click **Create** at the bottom of the blade.  The storage account will then be created and pinned to your Startboard in the portal.
15. Once the storage account is created you can open the account blade.
>The storage account blade has a lot of information on it.  The monitoring will not fill in unless diagnostics is enabled. 
16. Click on the **Keys** tile.  A new blade will appear with the access keys for your storage account.  
17. Keep the browser window open or copy one of the account access keys for use later in the HOL.


## Adding the Code to the website
Now that we have a storage account we can now add in the code to the website to write uploaded images to Blob storage and have the website also point to the images directly in Blob storage.

1. Open Visual Studio and open the **MyJunkYourStuff.sln** found in the root of the code repository or zip file.
2. Add the Azure Storage Client Library to the project.
	- Right-click on the project and select **Manage Nuget Packages**.
	- Make sure you select Online in the dialog on the left and then search for "Azure storage".  You may want to set NuGet to show "Stable Only" packages, or be sure to not select a pre-release library for storage.
	- Select **Windows Azure Storage** and click the **Install** button.
	- Close the Nuget dialog.
	> As of the time of writing the version number for the package was was 4.3.0, published 9/16/2014.
	> You can also install the package directly from the Nuget Package Console using:
	
	```PowerShell
	Install-Package WindowsAzure.Storage -Version 4.3.0    
	```
3. Add a new class named **AzureBlobImageRepository.cs** to the Models folder.
4. Paste the following code into the new class file:
	>You can copy and paste the contents of the file below, or you can add the existing AzureBlobImageRepository.cs class to your project using Visual Studio's Add Existing item.  The code file is located in the same directory as this ReadMe.
	
	```csharp
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
	```
	This class is used to write uploaded files for new locations to Blob Storage, and delete a file related to a location if the location is deleted.  Note that the credentials for the account are stored in the application settings so we need to add that information.
5. Open **web.config** and locate the appSettings element.  Add the following keys:
```xml
	<!-- ImageRepository -->
    <add key="Blob.storageAccountKey" value="{YourStorageKey}" />
    <add key="Blob.storageAccountName" value="{YourStorageAccountName}" />
    <add key="Blob.containerName" value="locationimages" />
```
	Replace the {YourStorageAccountName} with the name of the storage account you created above (including the curly braces being removed).  Do the same with replacing {YourStorageKey} with the access key you retrievied from the previous step of creating a storage account.
6. Open the **NinjectWebCommon.cs** file under the App_Start folder.
7. In the **RegisterServices** method comment out or remove the following line:
	```C#
	kernel.Bind<IImageRepository>().To<LocalImageRepository>();
	```
	And add the following line:
	```C#
	kernel.Bind<IImageRepository>().To<AzureBlobImageRepository>();
	```

	This swaps out the original local image repository with the one we are adding to work with Azure Blob Storage.  This repository is injected into the LocationsController and is used by that controller to perform the actions of writing a file and deleting when necessary.  Note that the **AzureBlobImageRepository** and the Edit feature of the website doesn't have functionality to deal with updating the image, that's an excercise left more to the reader (errr.. attendee?).  The code would be the same to replace an image as it is to upload it as you are simply overwriting the version that is there.  You may want to look at the Blob Snapshot feature to keep older versions as well.  See the article series referenced below in the Additonal Resources for some really good information on dealing with Azure Blob Storage. 

	> The **LocalImageRepository** class works just fine locally with IIS Express, but if you deploy it the Add and Delete will fail.  This is because the site needs to have permissions to modify the files in the */Content/locationimages/* directory.  It is not necessary for this HOL for the LocalImageRepository to work when deployed so the site deployment HOL doesn't take this into account.    

## Dealing with where the files live
Once files are pushed to Azure Blob Storage the website will need to know that the URLs for those files need to point to the publicly exposed images instead of a local directory.  A class already exists in the project to handle this.  A HTML Helper named **ImageHtmlUrlHelper** can be found in the Models folder.  By default this helper checks the web.config for the root path where location images are stored.  If it doesn't find this setting it will default to using a local path.

1. Open the **ImageHtmlUrlHelper.cs** class in the Models directory.
2. Note the static constructor sets a default if it can't find one in the web.config.  The default is the application root URL and /Content/locationimages/.   
3. Open the **web.config** file and locate the appSettings element.  Add the following keys:
	```xml
	<!-- Image Base Uri Source -->
    <add key="Image.baseUri" value="https://{YourStorageAccountName}.blob.core.windows.net/locationimages/" />
	```
	Replace the {YourStorageAccountName} with the name of the storage account you created above (including the curly braces being removed). This indicates the base URL of the images in Azure Storage.
4. Right-click on the web project and select **Publish...**.  If you followed the HOL for deploying the application previously all your setting should be the same and you simply need to click **Publish** on the dialog.  If you haven't deployed the site yet, you'll need to following the instructions in the [Deploying a simple site to Azure Websites](/HOLs/Azure Websites/ReadMe.md)
5. The site should open in a browser after the publish, but if not open your own browser window and navigate to the URL.
6. Click on **Locations** in the top nav bar of the site.
7. Click on **Create New** at the top of the screen above the list of current locations.
8. Provide a title and Start Time for your location.  
	- The Start time is expecting a date format, you can add a time if you'd like. For example, 1/12/2015 9:00 AM.
9. Click the **Browse** button and select a png or jpg image that is less than 4 MB in size.  A sample image for you to use is located in the source repository in a directory called TestImages.  The name of the file is LegoShip.jpg.
10. Fill in a description, Junker Name and address if you like.  These aren't required fields.
11. Click the **Create** button.
12. Once the new location is created you can scroll to the bottom of the Locations index page and see your new location.
13. If you inspect the source of the page, or look at the properties of the image on your location you'll see it is being sourced directly from blob storage. 

> Note the ImageHtmlUrlHelper can be updated and expanded upon to deal with all image or static files rather than just location images.  This is really helpful if you also want to use a Content Delivery Network (CDN) for some of your content.  Think of this as an example of what's possible and build out what you need. For example, you could create another setting value that turns the use of the helper on and off for local development, or even have a setting that has the helper perform a failover/fallback mechanism to multiple file stores that have the file data duplicated for redundancy.

## Looking at the files using a Storage Explorer (Optional)
The website is now pushing uploaded files to Blob storage, but what if you need to do some maintenance work on these files?  There are numerous Blob explorer tools out there, including some free ones.  This part of the HOL will show you how to use Visual Studio to see the list of files, but the Additional Resources section also has a link to more Storage Explorers to try out.  If you are going to be using Azure Storage it is **highly** recommended that you get a good storage explorer tool, otherwise it's a bit like working with SQL Server without SSMS.

1. In Visual Studio expand the Server Explorer.  If it isn't visible you can find it under the **view** menu.
2. Expand the Azure node.  This is installed as part of the Azure SDK for Visual Studio.  It may prompt you to log in.  This would be the same credentials you used to log into the Azure preview portal.
3. Expand the **Storage** node.  This may take a second or two to refresh and fetch the list of accounts.
4. Expand the storage account you used to host your images and expand the **Blobs** node.
5. Double-click the locationimages container.
6. Visual Studio will show you a list of the blobs in the container.

You can double click on a file to download it, upload new files, etc. from this tool.



## Additional Resources
In this HOL we updated a simple web site to use Azure Blob Storage for images, but we also could have continued on to host all of our site's static files via Blob storage.  We could also even take advantage of the Azure CDN to push the content out even closer to our users, or even a different CDN from another provider.  To continue your learning after the workshop we recommend the following links for additional knowledge in this area:

- [Using CDN for Azure ](http://azure.microsoft.com/en-us/documentation/articles/cdn-how-to-use/)- A How To guide by Microsoft on using their Content Delivery Network service.
- [Cerebrata Azure Explorer](http://www.cerebrata.com/products/azure-explorer/introduction) - A FREE Azure Blob  explorer from Cerebrata.  Cerebrata also has a more robust tool, Azure Management Studio, which can also work with Tables, Queues, Service Bus and more.  
- [Storage Explorers List](http://blogs.msdn.com/b/windowsazurestorage/archive/2014/03/11/windows-azure-storage-explorers-2014.aspx) - A post by the Azure Storage team with links to several storage explorers.  Some are free, some are paid and have trials.  We highly recommend trying several to find the one that works for you before buying one.
- [Azure Blob Storage](http://justazure.com/azure-blob-storage-part-one-introduction/) series by Robin Shahan on [JustAzure.com](http://justazure.com/)
- [Azure Storage Diagnostics/Metrics](Monitor a Storage Account in the Azure Management Portal ) - In the Preview portal it is called Diagnostics, and in much of the documentation to date you'll see this called Storage Metrics.  The article will talk about how these work and how to turn them on using the current (not preview) portal.  These are good ways to find out how your content is being used, if you are approaching a throughput limit and if you're table partitioning is bad.