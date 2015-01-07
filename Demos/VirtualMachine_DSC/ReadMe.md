# ReadMe #

This Azure Virtual Machines demo will walk through several key aspects of creating and customizing a new Azure VM. Specifically, the following topics will be covered:
- Create a VM
	- Add data disk
	- Add endpoints
	- Add extensions
- Use Visual Studio to Deploy a Web Site to an Azure VM

There are two common approaches in working with Azure Virtual Machines: the Azure management portal and command line tools. For the purposes of this demo, the Azure Preview Portal (http://portal.azure.com) and PowerShell will be used. Alternatives include the Azure Management Portal (http://manage.windowsazure.com), the Azure Cross-Platform Command-Line Interface (xplat-cli), REST API, and third-party tools.

**Estimated Time: 80 minutes** (including time to provision VM)


## Prerequisites ##
In order to complete this lab you will need the following:
- **Azure PowerShell cmdlets** 
	- Version used in workshop was 0.8.12. 
	- Use either Web Platform Installer or Windows Standalone (https://github.com/Azure/azure-powershell/releases)
- **Azure SDK 2.5 (for Visual Studio 2013)** 
	- Obtain from http://azure.microsoft.com/en-us/downloads/
- **Visual Studio 2013 Pro with Update 4 (or higher)** 
	- Download 90-day free trial from http://www.visualstudio.com/en-us/downloads/download-visual-studio-vs.aspx 
	- Alternative: Visual Studio Community 2014 with Update 4 (http://www.visualstudio.com/en-us/downloads/download-visual-studio-vs.aspx
- **Azure Subscription** 
	- Free Month Trial: sign up via http://azure.microsoft.com/en-us/pricing/free-trial/ 
	- OR MSDN Benefits: http://azure.microsoft.com/en-us/pricing/member-offers/msdn-benefits/
	- OR Use an existing subscription

## Azure Preview Portal ##

**Create a VM**

1. Log into the Azure Preview Portal at http://portal.azure.com
2. Click on **NEW** at bottom right of portal.
3. Select **Windows Server 2012 R2 Datacenter**. Alternatively, select **Everything** to browse for all available resources, select **Virtual Machines** and then search for **Window Server 2012 R2 Datacenter** .
5. On the **Create VM** blade, enter values for the Host Name, User Name, and Password (administrative credential). Also provide an appropriate Pricing Tier, Resource Group and Location. Optionally configure additional settings such as the Virtual Network, Azure Storage, etc.
6. Click the **Create** button when finished. Wait for the VM to finish provisioning. Provisioning a VM will take several minutes.

**Add Data Disks**

In this section you will add two new data disks to the VM. The disks will be unformatted and will need to be formatted prior to use. A custom script extension will be used to format the disks. Alternatively, you may use Remote Desktop to access the VM and format the disks manually.

After the VM has been successfully provisioned, execute the following steps:

1. Navigate to the desired VM and select the **Disks** part. By default, the VM should have one disk (the OS disk).
2. On the **Disks** blade, select **Attach New** to attach a new data disk.
3. On the **Attach a new disk** blade, select the desired storage account and container to persist the new VHD (recommendation for this demo - use the same storage account and container as the OS disk). This can be the same storage account as the OS disk, or another storage account.  Also provide the Disk File Name, Size, and Host Caching settings (more info on [Azure disks and host cache settings](http://blogs.msdn.com/b/windowsazurestorage/archive/2012/06/28/exploring-windows-azure-drives-disks-and-images.aspx)).
4. Click **OK** to add the new data disk to the VM.

**Add Endpoints**

In this section you will add two new endpoints to the VM: HTTP and HTTPS.

After the VM has been successfully provisioned, execute the following steps:

1. Navigate to the desired VM and select the **Endpoints** part. By default,VMs will have two endpoints enabled: Remote Desktop and WinRM.
2. On the **Endpoints** blade, select **Add**.
3. On the **Add an endpoint** blade, provide a name for the endpoint, protocol, public and private port. Create two endpoints (one at a time): *HTTP, TCP, 80, 80* and *HTTPS, TCP, 443, 443*.

You will need to wait until the endpoints are added before being able to proceed to make additional VM modifications (i.e. before adding the extensions).

**Add the Extensions**

In this section, two Azure VM extensions will be added: **PowerShell Desired State Configuration** and **Custom Script**. The PowerShell Desired State Configuration (DSC) extension will be used to configure specific Windows features applicable to a web server. The Custom Script extension will be used to format the previously added data disks. Note that the Custom Script extension executes a PowerShell (.ps) file that can perform a wide range of actions. Formatting disks is just example.

After the VM has been successfully provisioned, execute the following steps:

1. Navigate to the desired VM and select the **Extensions** part (in the Configuration section on the blade).
2. On the **Extensions** blade, select **Add** from the top of the blade, and then select the appropriate extension to add to the VM.

*Custom Script*

1. Select the **Custom Script** extension from the list of available extensions.
2. Read the information about the extension and then select **Create**.
3. On the **Add Extension** blade, browse to and select the *formatdisk.ps1* script file. There are no arguments to provide for this script.
4. Click **Create**.

*PowerShell Desired State Configuration*

1. Select the **PowerShell Desired State Configuration** extension from the list of available extensions.
2. Read the information about the extension and then select **Create**.
3. Open the Azure PowerShell console and execute the following command to create a configuration module (.zip file). Be sure to change the path to be relative to your machine. ```Publish-AzureVMDscConfiguration -ConfigurationPath "C:\CodeMash2015\Demos\VirtualMachine_DSC\ConfigureWebServer.ps1" `
 -ConfigurationArchivePath C:\CodeMash2015\Demos\VirtualMachine_DSC\ConfigureWebServer.ps1.zip -Force```
4. In the **Module-qualified Name of Configuration** textbox, provide the module configuration in the form of SCRIPT.PS1\CONFIGURATION_NAME (e.g. ConfigureWebServer.ps1\ConfigureWebServer).
5. Click **Create**. The VM will reboot as part of configuring the machine.



*Note: The Custom Script extension can also be used for Linux VM. See http://azure.microsoft.com/blog/2014/08/20/automate-linux-vm-customization-tasks-using-customscript-extension/ for more information.*


**Validatation**

You can use Remote Desktop to log into the new Azure VM. Verify the following:


- Server roles are installed as defined in the DSC script (*ConfigureWebServer.ps1*).
- All added data disks are attached and formatted.


*Estimated Time to Completion: 35 minutes.*


## PowerShell ##
All the steps manually completed previously can be automated via a PowerShell script.


**Steps**

1. Open the PowerShell ISE.
2. Execute `Add-AzureAccount` to connect your Azure subscription(s) to the current PowerShell session. If you have more than one Azure subscription, execute `Get-AzureSubscription -Current` to view the current subscription. To change subscriptions, use `Select-AzureSubscription -SubscriptionName` providing the name of the desired subscription.
3. Copy and paste the following code (also available in *CreateVirtualMachine.ps1* script) into the editor.
4. Change the storage account name and service name to be appropriate for your account.
5. Change the path to *ConfigureWebServer.ps1* to be appropriate for your local machine.
6. Execute the script.
7. When prompted, provide the username and password for the administrative account.

```
cls

$DebugPreference = "SilentlyContinue"
$VerbosePreference = "Continue"

# This script requires Azure PowerShell 0.8.6 or later.


# Uncomment the following line to view the available PowerShell DSC extension details
#Get-AzureVMAvailableExtension -Publisher Microsoft.PowerShell

$storageAccountName = "[YOUR-STORAGE-ACCOUNT]"
$serviceName = "[YOUR-SERVICE-NAME]"
$vmName = "codemash-dsc-1"

$storageKey = Get-AzureStorageKey -StorageAccountName $storageAccountName
$storageContext = New-AzureStorageContext -StorageAccountName $storageAccountName -StorageAccountKey $storageKey.Primary


# Default storage container is 'windows-powershell-dsc'
Publish-AzureVMDscConfiguration -ConfigurationPath "C:\CodeMash2015\AzureWorkshop\Demos\VirtualMachine_DSC\ConfigureWebServer.ps1" -StorageContext $storageContext -Force

# Save locally instead of publishing to Azure storage
#Publish-AzureVMDscConfiguration -ConfigurationPath "C:\CodeMash2015\AzureWorkshop\Demos\VirtualMachine_DSC\ConfigureWebServer.ps1" -ConfigurationArchivePath C:\CodeMash2015\AzureWorkshop\Demos\VirtualMachine_DSC\ConfigureWebServer.ps1.zip -Force



# Find the most recent Windows Server 2012 Datacenter image from the Azure image gallery
$image = Get-AzureVMImage `
    | where { ($_.PublisherName -ilike "Microsoft*" -and $_.ImageFamily -ilike "Windows Server 2012 R2 Datacenter" ) } `
    | sort -Unique -Descending -Property ImageFamily `
    | sort -Descending -Property PublishedDate `
    | select -First(1)


# Get the administrative credentials to use for the virtual machine
Write-Verbose "Prompt user for administrator credentials to use when provisioning the virtual machine(s)." 
$credential = Get-Credential -Message "Enter the username and password for the virtual machine administrator."
Write-Verbose "Administrator credentials captured.  Use these credentials to login to the virtual machine(s) when the script is complete."


# Create configuration details for the VM
$vm = New-AzureVMConfig -Name $vmName -InstanceSize Basic_A1 -ImageName $image.ImageName `
    | Add-AzureProvisioningConfig -Windows -AdminUsername $credential.UserName -Password $credential.GetNetworkCredential().Password `
    | Add-AzureDataDisk -CreateNew -DiskSizeInGB 10 -DiskLabel "disk 1" -LUN 0 `
    | Add-AzureDataDisk -CreateNew -DiskSizeInGB 10 -DiskLabel "disk 2" -LUN 1 `
    | Add-AzureEndpoint -Name "HTTP" -LocalPort 80 -PublicPort 80 -Protocol tcp `
    | Add-AzureEndpoint -Name "HTTPS" -LocalPort 443 -PublicPort 443 -Protocol tcp

# Set PowerShell Desired State Configuration (DSC) extenstion
$vm | Set-AzureVMDscExtension -ConfigurationArchive "ConfigureWebServer.ps1.zip" -ConfigurationName "ConfigureWebServer"

# Custom script extension
$vm | Set-AzureVMCustomScriptExtension -ContainerName "vmstartup" -FileName "formatdisk.ps1" -Run "formatdisk.ps1"


# Create the VM
New-AzureVM -VMs $vm -Location "West US" -ServiceName $serviceName -WaitForBoot


```

More information on the PowerShell DSC extension can be found at:
- http://blogs.msdn.com/b/powershell/archive/2014/08/07/introducing-the-azure-powershell-dsc-desired-state-configuration-extension.aspx

More information on the Custom Script extension can be found at:
- http://msdn.microsoft.com/en-us/library/azure/dn781373.aspx
- http://azure.microsoft.com/blog/2014/04/24/automating-vm-customization-tasks-using-custom-script-extension

*Estimated Time to Completion: 8 minutes.*


## Use Visual Studio to Deploy a Web Site to an Azure VM ##

1. Using Visual Studio, create a basic ASP.NET web site.
2. In Solution Explorer, right-click on the ASP.NET web site project and select **Publish**.
3. In the **Publish Web** wizard, first select a publish target. Expand the More Options section to make **Microsoft Azure Virtual Machines** visible.
4. If not signed into Azure already, you will be prompted to do so.
5. The **Select Existing Virtual Machines** dialog will list all existing virtual machines for which Web Deploy is enabled. Select an existing VM or click **New** to create a new VM.  For this walkthrough, create a new VM.
6. On the **Create virtual machine on Microsoft Azure** wizard, complete the necessary fields to create a new VM. For the image, select "Windows Server 2012 R2 Datacenter".
7. Wait for the VM to be provisioned - this will take several minutes. At this point, Microsoft Azure is creating a new Azure VM and installing the WebDeployForVSDevTest extension. See http://msdn.microsoft.com/en-us/library/azure/dn606311.aspx for more information.
8. In the Azure Preview Portal, browse to the VM to view the extension.
9. After the VM is provisioned and the extension installed, right-click on the ASP.NET web site project and select **Publish**. Visual Studio should automatically select the recently created Azure VM and populate the Web Deploy settings.
10. Click **Validate Connection** to ensure the settings are correct.
11. If prompted, accept the certificate validation error message.
12. Click **Next** twice.
13. Click **Publish** to start the Web Deploy publish/sync process.


Alternatively, when creating a new ASP.NET web site project, select the option in Visual Studio to host the web site in the cloud on Azure Virtual Machines. This will instruct Visual Studio to create deployment scripts to create a Web Deploy package and a new Azure VM configured with Web Deploy. For more information, please refer to http://msdn.microsoft.com/en-us/library/dn642480.aspx.

	
*Estimated Time to Completion: 35 minutes.*
