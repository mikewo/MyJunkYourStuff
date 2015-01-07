cls

$DebugPreference = "SilentlyContinue"
$VerbosePreference = "Continue"

# This script requires Azure PowerShell 0.8.6 or later.


# Uncomment the following line to view the available PowerShell DSC extension details
#Get-AzureVMAvailableExtension -Publisher Microsoft.PowerShell

$storageAccountName = "codemash2015"
$serviceName = "codemash-dsc-demo"
$vmName = "codemash-dsc-1"

$storageKey = Get-AzureStorageKey -StorageAccountName $storageAccountName
$storageContext = New-AzureStorageContext -StorageAccountName $storageAccountName -StorageAccountKey $storageKey.Primary


# Default storage container is 'windows-powershell-dsc'
Publish-AzureVMDscConfiguration -ConfigurationPath "C:\Projects\CodeMash2015\azure-continuum-workshop\Demos\VirtualMachine_DSC\ConfigureWebServer.ps1" -StorageContext $storageContext -Force

# Save locally instead of publishing to Azure storage
#Publish-AzureVMDscConfiguration -ConfigurationPath "C:\Projects\CodeMash2015\azure-continuum-workshop\Demos\VirtualMachine_DSC\ConfigureWebServer.ps1" -ConfigurationArchivePath C:\Projects\CodeMash2015\azure-continuum-workshop\Demos\VirtualMachine_DSC\ConfigureWebServer.ps1.zip -Force



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
