# Deploying a simple site to Azure Websites #

In this Hands on Lab (HOL) you will publish a simple web site to Azure Websites using web deploy.  In addition to Web Deploy there are several other options for deployment, such as Git, Visual Studio Online, BitBucket, FTP.  

This lab assumes that while you may not have experience with Microsoft Azure, you do have some familiarity with Visual Studio and C#.

**Estimated Time: 5 minutes**

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
 
## Running the site locally

First we will look at the simple website we will be deploying.



1. Open Visual Studio and open the **MyJunkYourStuff.sln** found in the root of the code repository or zip file.
2. Ensure the MyJunkYourStuff website project is set as the Start up Project.  If it isn't then in the Solution Explorer of Visual Studio right click the project and select "**Set as Startup Project**" option.  
3. Press F5, or select the Run button from the command bar to run the solution.  This should force a restore of any Nuget Packages, compile the application and start up the website locally using IIS Express.
4. You can see this is a simple web site devoted to posting garage sale locations.
5. You can close the browser or stop the application running in Visual Studio when you are finished.


## Creating an Azure Website
Next we will create an Azure Website to host our simple site.  The HOL walks you through setting the website up by hand in order to tour the Azure Portal some, but the tools in Visual Studio can also create this for you automatically when you deploy. 


1. Open a browser and navigate to the Azure Preview Portal: https://portal.azure.com.
>Note that you can accomplish this HOL using the current *Full* Azure Portal as well, but the steps would be slightly different.
2. Log in to the Portal using credentials that have access to an Azure subscription.
3. On the bottom left of the screen click the big **New** button.  
4. Select ***Everything*** from the top of the list that appears.
5. Select **Web** from the categories.
6. Select **Website** by Microsoft, usually found under the "Recommended" area.
7. Click **Create** on the blade describing the Website offering.
8. Type a name for the website.  This name must be unique globally as it will become the URL for your hosted site.  A green checkmark will appear if the name is acceptable.
9. For the Web Hosting Plan make sure one is selected that indicates free.
10. Note the resource group is created for you or you can select an already created one.
11. Verify you are creating this for the subscription you intend.
12. Set the location you wish to deploy to (which may be set for you based on the Web Hosting Plan you select).
13. Click **Create** at the bottom of the blade.  The site will then be created and pinned to your Startboard in the portal.
14. Once the site is created you can open the website blade.
15. Click the **Browse** button from the command bar at the top of the blade.

These steps have created an empty Azure website for you to host your site in.

## Deploying the site using WebDeploy from Visual Studio

Finally we will push our simple website out to the site we just created.
1. Return to Visual Studio that has the **MyJunkYourStuff** solution open.
2. Right-click on the web project and select **Publish...**.
3. From the dialog Select a Publish Target click on **Microsoft Azure Websites**.
4. You may be prompted to log in at this point to your Azure Account.  This is the same credentials you used when logging into the portal in the creation of the Azure Website above.  Once you are logged in the list of websites in the dropdown should be populated.  
5. Select the website you created in the above step from the drop down and click **OK**.  This will pull down the publish credentials specific to your Azure account.
6. Click the **Next** button.  
7. Note that on the next screen you can modify settings for Web Deploy, such as deleting additional files at the destination that aren't in the source, or modify the Configuration to be deployed (Release vs. Debug).
6. Click the **Publish** button.
7. Once the app finishes deploying a browser should open to the deployed web site URL.  If it doesn't open your own browser and test the URL to ensure the deployment worked.

## Additional Resources
In this HOL we deployed a simple web site using Web Deploy, but there are numerous ways you can deploy to Azure Websites, including using continuous delivery, which is outside the scope of this HOL.  To continue your learning after the workshop we recommend the following links for additional knowledge in this area:
- [Azure Website documentation](http://azure.microsoft.com/en-us/documentation/services/websites/) - Plenty of other tutorials and guides on the main Azure site.
- [How to Deploy An Azure Website](http://azure.microsoft.com/en-us/documentation/articles/web-sites-deploy/) - Other ways to perform a deployment, including FTP, Git, Mercurial and more.
- [Project Kudu](https://github.com/projectkudu/kudu) - This is the engine that handles deployments to Azure Websites when pushing via Mercurial or Git.  It is very flexible, giving you hooks to tap into when you do a push to a repo and before the site is finished deploying. Great for when you are doing continuous delivery.
- [Staged Deployment on Microsoft Azure Websites](http://azure.microsoft.com/en-us/documentation/articles/web-sites-staged-publishing/) - a tutorial on how to set up deployment slots for a website.  
- [Video: Enabling Testing in Production in Azure Websites](http://channel9.msdn.com/Shows/Web+Camps+TV/Enabling-Testing-in-Production-in-Azure-Websites) - Despite having a slightly terrifying name, this feature of Azure websites is really nice.  It lets you setup some A-B testing scenarios and more.


