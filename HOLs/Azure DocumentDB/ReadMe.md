# ReadMe #

In this Hands on Lab (HOL) you will modify the existing MyJunkYourStuff sample project to leverage [Azure DocumentDB](http://azure.microsoft.com/en-us/documentation/services/documentdb/) as a database instead of the default in-memory repository. Why? Well, computer memory has this odd forgetfulness tendency. 

This lab assumes that while you may not have experience with Microsoft Azure, but you do have some familiarity with Visual Studio and C#.

There are two main tasks as part of this HOL:
- Create a DocumentDB account
- Modify the MyJunkYourStuff project to use DocumentDB

Let's get started!

**Estimated Time: 30 minutes** *(including DocumentDB provisioning time)*

## Prerequisites ##
In order to complete this lab you will need the following:
- **Azure SDK 2.5 (for Visual Studio 2013)** 
	- Obtain from http://azure.microsoft.com/en-us/downloads/
- **Visual Studio 2013 Pro with Update 4 (or higher)** 
	- Download 90-day free trial from http://www.visualstudio.com/en-us/downloads/download-visual-studio-vs.aspx 
	- Alternative: Visual Studio Community 2014 with Update 4 (http://www.visualstudio.com/en-us/downloads/download-visual-studio-vs.aspx
- **Azure Subscription** 
	- Free Month Trial: sign up via http://azure.microsoft.com/en-us/pricing/free-trial/ 
	- OR MSDN Benefits: http://azure.microsoft.com/en-us/pricing/member-offers/msdn-benefits/
	- OR Use an existing subscription
- **The source for the web application**
	- You can use Git to fork/clone our repository at https://github.com/mikewo/MyJunkYourStuff
	- OR You can simply download the code as a Zip at the same address if you aren't familiar with Git.


**Disclaimer**
This is a demonstration project. **Do not use as production quality code.** There are many important areas missing from the majority of this code (robust error handling, logging, unit tests, etc.).  This HOL is intended to show you how to use basic features of Azure DocumentDB.

## Create a DocumentDB account ##
1. Log into the Azure Preview Portal at http://portal.azure.com
2. Click on **NEW** at bottom right of portal.
3. Select **DocumentDB**.
4. Enter and **Id** that will be part of the URL for the DocumentDB account. This must be globally unique (*note: the Azure Preview Portal does not validate the uniqueness on the creation screen*).
5. Select the desired **Optional Configuration** (capacity unit), **Resource Group**, **Subscription**, and **Location** (during preview, only West US, North Europe, and West Europe are available). For this HOL, it is recommended to select West US.
6. Click **Create**.

*There is no free option for DocumentDB. You will be charged for DocumentDB usage and data transfer out of the Azure datacenter. Delete the DocumentDB account to stop the billing! View standard Azure DocumentDB pricing details at http://azure.microsoft.com/en-us/pricing/details/documentdb/.*

*Estimated Time to Completion: 10 minutes.*
>While Azure is provisioning your DocumentDB account, you may proceed to the next section to save a little time.

## Modify the MyJunkYourStuff project ##
In this section you will modify the MyJunkYourStuff project to use DocumentDB. You will add and use a new repository, *DocumentDBRepository*. You will also make a few other modifications necessary to better support DocumentDB.

**Add DocumentDB client library**

In order to develop against DocumentDB using the .NET SDK, you will need to include the client library.  The DocumentDB client library depends on Newtonsoft.Json for JSON serialization. NuGet will automatically configure this dependency.

1. Right-click on the MyJunkYourStuff project and select **Manage NuGet packages...** Be sure to select "Include Prerelease".
2. Search for "docdb"
3. Select and install the **Microsoft Azure DocumentDB Client Library**. *Note: this is a Prerelease library.*

*Alternative (NuGet Package Console)*

```PowerShell
Install-Package Microsoft.Azure.Documents.Client -Version 0.9.1 -Prerelease
```

**Add DocumentDBRepository.cs**

In this section you will add the DocumentDBRepository class to your MyJunkYourStuff project. The DocumentDBRepository class is located in the repository at *HOL/Azure DocumentDB/DocumentDBRepository.cs*.

Add the DocumentDBRepository.cs to the Models folder in the MyJunkYourStuff project.

>You will need to remove the LocationInMemoryRepository class from your project. This class will not compile once changes are made to the Location class (below). Alternatively, you may change the LocationInMemoryRepository to support the new DateEpoch class (below).

**Add DateEpoch class**

Since DocumentDB does not handle dates like .NET does, you need to change how *DateTime* is represented. For an in-depth explanation, please see http://blogs.msdn.com/b/documentdb/archive/2014/11/18/working-with-dates-in-azure-documentdb.aspx.

Add the DateEpoch and Extensions class to your project (in the Models) folder. The classes can be found below, or copied from the *HOL/Azure DocumentDB/DateEpoch.cs* file.

```C#
public class DateEpoch
{
    [JsonProperty(PropertyName = "date")]
    public DateTime Date { get; set; }

    [JsonProperty(PropertyName = "epoch")]
    public int Epoch
    {
        get
        {
            return (this.Date.Equals(null) || this.Date.Equals(DateTime.MinValue))
                ? int.MinValue
                : this.Date.ToEpoch();
        }
    }
}

public static class Extensions
{
    public static int ToEpoch(this DateTime date)
    {
        if (date == null) return int.MinValue;
        DateTime epoch = new DateTime(1970, 1, 1);
        TimeSpan epochTimeSpan = date - epoch;
        return (int)epochTimeSpan.TotalSeconds;
    }
}
```

**Modify Models/Location.cs**

There are a few minor modifications you will need to make to the *Location* class. First, JSON documents typically use camelCase for properties (instead of PascalCase like .NET). The DocumentDB client library uses JSON.NET to serialize/deserialize documents. Adding the *JsonProperty* attribute to each Location property will ensure that JSON.NET serializes the properties as indicated.

DocumentDB automatically includes an "id" property that is user defined. In order to use the default "id" property, be sure to add `[JsonProperty(PropertyName = "id")]` to the Location.Id property.

Add the *JsonProperty* attribute to each property, or replace the existing *Location* class with the content below.

```C#
public class Location
{
    [JsonProperty(PropertyName = "id")]
    public Guid Id { get; set; }

    [JsonProperty(PropertyName = "title")]
    public string Title { get; set; }

    [JsonProperty(PropertyName = "startTime")]
    public DateEpoch StartTime { get; set; }
    
    [JsonProperty(PropertyName = "mainImageName")]
    public string MainImageName { get; set; }

    [JsonProperty(PropertyName = "junkerName")]
    public string JunkerName { get; set; }

    [JsonProperty(PropertyName = "description")]
    public string Description { get; set; }

    [JsonProperty(PropertyName = "address")]
    public string Address { get; set; }
}
```


**Modify Views (Home/Index.cshtml, Locations/*)**

As the *Location.StartTim*e has been change from a *DateTime* to a *DateEpoch* type, you need to make this change to ensure that both the the Date and Epoch values are not rendered. You will need to change several Views to properly display the StartTime for each Location. 

*Home/Index.cshtml*

Add the Date property to *item.StartTime* on line 21. The final code should resemble the snippet below.
```C#
@foreach (var item in Model)
{
    @String.Format("{0:MM/dd/yy}", item.StartTime.Date) 
    @Html.Raw("&nbsp;-&nbsp;")
    @Html.ActionLink(item.Title, "Details", "Locations", new { id = item.Id }, null) 
    <br />
}
```

*Locations/Create.cshtml*

Add the Date property to the *model.StartTime* edit control on line 29. The final code should resemble the snippet below.
```C#
<div class="form-group">
    @Html.LabelFor(model => model.StartTime, htmlAttributes: new { @class = "control-label col-md-2" })
    <div class="col-md-10">
        @Html.EditorFor(model => model.StartTime.Date, new { htmlAttributes = new { @class = "form-control" } })
        @Html.ValidationMessageFor(model => model.StartTime, "", new { @class = "text-danger" })
    </div>
</div>
```

*Locations/Delete.cshtml*

Add the Date property to the *model.StartTime* display control on line 27. The final code should resemble the snippet below.
```C#
<dd>
    @Html.DisplayFor(model => model.StartTime.Date)
</dd>
```

*Locations/Details.cshtml*

Add the Date property to the *model.StartTime* display control on line 28. The final code should resemble the snippet below.
```C#
<dd>
    @Html.DisplayFor(model => model.StartTime.Date)
</dd>
```

*Locations/Edit.cshtml*

Add the Date property to the *model.StartTime* edit control on line 32. The final code should resemble the snippet below.
```C#
<div class="form-group">
    @Html.LabelFor(model => model.StartTime, htmlAttributes: new { @class = "control-label col-md-2" })
    <div class="col-md-10">
        @Html.EditorFor(model => model.StartTime.Date, new { htmlAttributes = new { @class = "form-control" } })
        @Html.ValidationMessageFor(model => model.StartTime, "", new { @class = "text-danger" })
    </div>
</div>
```

*Locations/Index.cshtml*

Add the Date property to the *mode.StartTime* display control on line 47. The final code should resemble the snippet below.
```C#
<td>
    @Html.DisplayFor(modelItem => item.StartTime.Date)
</td>
```

**Modify NinjectWebCommon.cs**

To instruct the MyJunkYourStuff project to use the new DocumentDB repository, you need to change Ninject to use *DocumentDBRepository* instead of *LocationMemoryRepository* whenever an *ILocationRepository* is requested.

In the *App_Start/NinjectWebCommon.cs*, modify the *RegisterServices* method to use *DocumentDBRepository* as indicated in the code below.

```C#
private static void RegisterServices(IKernel kernel)
{
    //Create the in-memory version of the repository, only one for the lifetime of the application.
    //This is just for the in-memory sample only, before we insert persistance, not for production use.
    kernel.Bind<IImageRepository>().To<AzureBlobImageRepository>();
	kernel.Bind<ILocationRepository>().To<DocumentDBRepository>();
);
}  
```
>Note that the AzureBlobImageRepository class was added in another HOL. If you don't already have this then you can leave the LocalImageRepostiory class that is there.

**Modify web.config**

The configuration information for DocumentDB is located in the project's web.config file. You will need to add the DocumentDB account endpoint and authorization key. You can obtain both from the Azure Preview Portal by clicking on the Keys tile on the Document DB Account blade.

Add the following configuation to the *web.config* file in the appSettings section.

```xml
<!-- DocumentDB -->
<add key="endpointUrl" value="https://{YOUR-DOCUMENTDB-ACCOUNT}.documents.azure.com:443/" />
<add key="authKey" value="{YOUR-ACCOUNT-KEY}" />
<add key="databaseId" value="MyJunkYourStuff" />
<add key="collectionId" value="Junk" />
```

**DocumentDBBootstrapper**

You now need to set up the DocumentDB database and document collection. A separate project, DocumentDBBootstrapper, has been provide at *HOL/Azure DocumentDB/DocumentDBBootstrapper* that will ensure the database and collection are properly configured. 


1. Add the project to the existing MyJunkYourStuff solution.
2. You will also need to edit the *app.config* in DocumentDBBootstrapper project to reference your DocumentDB account and authorization key (same as in your *web.config*)
3. Rebuild the DocumentDBBootstrapper project to restore any missing NuGet references.
>If solution fails to build with a reference error, you may need to remove the readd the DocumentDb reference via NuGet.
4. Run the DocumentDBBootstrapper project from Visual Studio to create the database, document collection, and sample data.
5. In the Azure Preview Portal, select your DocumentDB account and then the **Document Explorer** part. You should now be able to see your database and collection created, as well as populated with test data.

**Test The Site**

We should now push our changes to our deployed website and test.

1. Run the MyJunkYourStuff website locally. The site should now connect to DocumentDB for all data (except images - which will use Azure Blob storage).
2. Change a item and then browse to the **Document Explorer** in the Azure portal to view those changes in DocumentDB.

0ptionally you can also push these changes out to your deployed Azure site.
3. Right-click on the web project and select Publish.... If you followed the HOL for deploying the application previously all your setting should be the same and you simply need to click Publish on the dialog. If you haven't deployed the site yet, you'll need to following the instructions in the [Deploying a simple site to Azure Websites](/HOLs/Azure Websites/ReadMe.md).
4. The site should open in a browser after the publish, but if not open your own browser window and navigate to the URL.

*Estimated Time to Completion: 15 minutes.*

## Azure DocumentDB Resources ##
To continue your learning after the workshop we recommend the following links for additional DocumentDB knowledge.
- [Azure DocumentDB documentation](http://azure.microsoft.com/en-us/documentation/services/documentdb/)
- https://github.com/Azure/azure-documentdb-net
- [Azure DocumentDB .NET Code Samples](https://code.msdn.microsoft.com/windowsazure/Azure-DocumentDB-NET-Code-6b3da8af/)
- [Query Using DocumentDB SQL](http://www.documentdb.com/sql/tutorial)
	- [DocumentDB SQL Query Language](http://msdn.microsoft.com/en-us/library/azure/dn782250.aspx)
	- [Query DocumentDB](http://azure.microsoft.com/en-us/documentation/articles/documentdb-sql-query)
- [Intro to Azure DocumentDB](http://blogs.msdn.com/b/cloud_solution_architect/archive/2014/12/07/intro-to-azure-documentdb.aspx) (Microsoft Cloud Solution Architect blog)
- [DocumentDB Studio](https://studiodocumentdb.codeplex.com/) (supporting material at [Vincent-Philippe Lauzon's blog](http://vincentlauzon.com/2014/10/31/querying-collections-with-documentdb-studio/))
