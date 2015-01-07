# Setting up Azure Traffic Manager#

In this Hands on Lab (HOL) you will configure Azure Traffic Manager to map traffic between two websites based on where the user is coming from.  We will also see what happens when one of the sites goes offline.    

This lab assumes that while you may not have experience with Microsoft Azure, you do have some familiarity with Visual Studio and C#.

> **WARNING**: This HOL will walk you through setting up two Azure websites behind the Azure Traffic Manager.  In order for Traffic Manager to map traffic to these Azure Websites they need to be hosted at the Standard Pricing Tier. This will cost you money if you are using a Pay as you Go Subscription.  If you have a MSDN benefits, the Free Trial account or credits from some other source the cost will come out of that first.  Overall, the HOL shouldn't cost more than a dollar or so if you remember to scale the sites back down at the end (and yes, we will remind you).

**Estimated Time: 20 minutes**

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

The HOL also walks through using the Azure Management Portal for some of the instructions (Not the Azure Preview Portal).  Note that the Azure Management Portal changes quite regularly with new updates and functionality.  Depending on when you are using these notes the instructions below may not match what you are seeing online.

The biggest thing you'll need to understand beyond setting up the Traffic Manager is what it means to your application if suddenly one of the sites goes down.  Yes, Traffic Manager will redirect your traffic, but what about your underlying data?  You need to plan for that separately from Traffic Manager.  We will talk about this in the workshop near the end.

## Preparing your websites to work with Traffic Manager
Each site that resides behind the Traffic Manager is simply an endpoint to the traffic manager.  They can be identical copies code wise, or not.  Traffic Manager doesn't care.  Currently Traffic Manager can point to:
- An Azure Cloud Service (both Web/Worker roles as well as Azure VMs)
- An Azure Website (hosted in Standard tier)
- An external address (this requires you to configure Traffic Manager using PowerShell, the Portal doesn't support this yet)

In order to complete this HOL you will need to deploy the website to two different regions and scale them to the Standard tier.

1. Open Visual Studio and open the **MyJunkYourStuff.sln** found in the root of the code repository or zip file. 
2. Follow the instructions in the [Deploying a simple site to Azure Websites](/HOLs/Azure Websites/ReadMe.md) HOL to create and deploy two instances of the website in separate locations.  If you have been doing these HOLs in order you will only need to create the second website and deploy to it as the first one will already be deployed.
3. Open a browser and navigate to the Azure Management Portal: https://manage.windowsazure.com.
4. Log in to the Portal using credentials that have access to an Azure subscription.
5. Select the websites tab from the menu on the left.
6. Select one of your websites from the list.
7. Click on the **Scale** tab for the website.
8. Under "Web Hosting Plan Mode" select the **Standard** option.
9. Keep the Instance size set to Small.  
10. Click the **Save** button.  
>You will be prompted that this change will start to incur cost, even if the sites are stopped.  It will also indicate if there are any other sites in the same region that will be moved to standard at the same time.
11. Click **Yes** to update the site to Standard.
12. Perform the scaling operation on the second site as well with steps 7-11.
13. Choose one of the sites and click on the **Configure** tab.
14. Scroll down until you see the **app settings **section.
15. Modify the **Location** entry to be something other than its current value of CodeMash. If you do not see a **Location** entry, add it and supply a value. 
> This value is displayed on every page of the website in the footer of the page next to the copyright notice.
16. Once both scale operations are complete and the one site has a different Location configuration value test both sites by going to thier URLs.  You should be able to see the Location value at the bottom of any of the pages so that you can tell the two sites apart from one another.  
> For something more noticeable you could change the background color in the css or add different text to the home view if you'd like, but these will require a code change and redeployment.

Now that we have two locations hosting up the same website we can create move forward with routing traffic to them behind the Traffic Manager.
 
## Creating & Configuring the Azure Traffic Manager Profile
The first step is to create the Azure Traffic Manager profile, which is comprised of a traffic load balancing method, a set of endpoints and some configuration. 

1. Open a browser and navigate to the Azure Management Portal: https://manage.windowsazure.com.
>Note that the new Azure Preview portal does not currently support Traffic Manager at the time of writing, so you must perform these actions in the Azure Management portal.  
2. Log in to the Portal using credentials that have access to an Azure subscription.
3. Scroll down through the categories on the left until you find the Traffic Manager.
3. On the bottom left of the screen click the big **New** button.  
4. Select the category **Networks Services**.
5. Select **Traffic Manager**.
6. Select **Quick Create**.
7. Type a name for the traffic manager profile.  This name must be unique globally as it will become the URL used by people to reach the content behind the Traffic Manager.  A green checkmark will appear if the name is acceptable.
8. Leave the default Load balancing method at **Performance**.
> The Load Balancing Methods determine how Traffic Manager handles the traffic to the registered endpoints.  
	- **Performance** means Traffic Manager will attempt to route a user to the endpoint that has the lowest latency for that user by passing back the address for that endpoint during a DNS lookup.  This is sometimes referred to as "the closest" endpoint, but it may not actually be the endpoint that is the shortest distance from the user.  It's all about latency.
	- **Round Robin** is for when you want to distribute traffic across multiple endpoints.  Keep in mind that this doesn't setup some sort of stickiness between the user and the endpoint.
	- **Failover** is for when you want one or more "backup" endpoints.  This is used when you have one endpoint that takes all of the traffic and if (when) that one fails the traffic will be then sent on to another endpoint.  As you will see in this HOL Traffic Manager also provides some failover options with the other methods as well.
9.  Select the newly created profile once it is provisioned.
10. Note down the URL on the Dashboard tab.  This is the publicly facing URL that you would provide to users, or the one you'd put behind a custom domain.  
11. Click on the **Endpoints** tab.
12. Click on **Add Endpoints**
13. In the dialog change the **Service Type** dropdown to **Website**.  
> You should see any websites in your subscription that are hosted at the Standard tier.  If you don't see any sites listed, make sure the sites are completed scaling to Standard and that they are in the same subscription as the profile you've created.
14. Select both sites by checking their box.
15. Click the checkmark at the bottom left of the dialog to accept the endpoints.
16. The endpoints screen should return and take a few moments to provision.
17. After the endpoints are finished provisioning open a browser and navigate to the URL for the traffic manager profile you've created.  It will be {somename}.trafficmanager.net.
> Note the footer at the bottom of the web page on the website to see which endpoint you were directed to. Since we are using Performance this should be consistent that you get directed to the same site as long as that site is running.

## Testing Azure Traffic Manager when Endpoints go down
Now that's simulate a problem and watch Traffic Manager handle the failure of one of the endpoints.

1. Open a browser and navigate to the Azure Management Portal: https://manage.windowsazure.com.
2. Log in to the Portal using credentials that have access to an Azure subscription.
3. Scroll down through the categories on the left until you find Website and select it.
4. Select the website you have been routed to when you use the Traffic Manager URL.
5. Press the **Stop** button at the bottom of the screen.  This will stop the website from responding to any request.
6. Return to your browser and go to your Traffic Manager URL.  
7. Note the location value in the footer to see that you've now been routed the other endpoint.
> Wait... what's this 503 error or "Web Site" is down message I'm seeing?
> Traffic Manager works off DNS, it doesn't actually route traffic through Traffic Manager servers.  If you see a 503 or Azure Website is down message it's because your browser is still using the cached value for the DNS entry that was handed to it the last time it tried to go the address.  To get around this you can open a new browser (using incognito mode or InPrivate browsing works really good for this) so that the browser performs a new lookup against Traffic Manager.
> How long a DNS entry is cached can have all sorts of variables to it, so remember that if you have a failover scenario like what we set up it might take a while for current users to actually see the result and be sent to another endpoint.  They may see it as a site failure for some time. 



## Friendly Reminder to Scale down your Sites
**DON'T FORGET TO SCALE YOUR WEBSITES BACK DOWN TO FREE OR DELETE THEM.**

As mentioned earlier in the HOL the Traffice Manager can only direct traffic to Azure websites if they are Standard or above, which means they aren't the free sites.  If you followed the instructions in this HOL you have two sites running in Standard and those should be scaled back down to Free unless you want to pay to keep them at Standard (or until your MSDN/Trial account credit runs out and they are turned off).  You can also delete the sites after removing their endpoints from the Traffic Manager profile.


## Additional Resources
In this HOL we configured the Azure Traffic Manager to help push traffic for our users to the *closest* site in order to improve the site experience.  We also tested what happens when one of the sites fails and how Traffic Manager can also help for High Availability and failover scenarios.  To continue your learning after the workshop we recommend the following links for additional knowledge in this area:
- [Traffic Manager pricing](http://azure.microsoft.com/en-us/pricing/details/traffic-manager/)
- [Azure Traffic Manager Nested Profiles](http://azure.microsoft.com/blog/2014/10/29/new-azure-traffic-manager-nested-profiles/) - Something released in late 2014 was the ability to nest Azure Traffic Manager profiles so that you can have a performance based profile that routed a user to the closest location, and then a nested round robin profile to distribute that load across different deployments at the location.  You can actually set up quite a few different scenarios with nested profiles.


