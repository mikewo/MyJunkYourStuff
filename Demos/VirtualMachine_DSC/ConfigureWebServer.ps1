
configuration ConfigureWebServer
{
    node Localhost
    {

        WindowsFeature WebServerRole
        {
            Ensure = "Present"
            Name = "Web-Server"
        }

        WindowsFeature WebAspNet45
        {  
            Ensure          = "Present"  
            Name            = "Web-Asp-Net45" 
            DependsOn = "[WindowsFeature]WebServerRole" 
        } 

        WindowsFeature WebMgmtTools
        {
            Name = "Web-Mgmt-Tools"
            Ensure = "Present"
            DependsOn = "[WindowsFeature]WebServerRole"
        }

        WindowsFeature WebMgmtConsole
        {
            Name = "Web-Mgmt-Console"
            Ensure = "Present"
            DependsOn = "[WindowsFeature]WebServerRole"
        }

        WindowsFeature WebMgmtService
        {
            Name = "Web-Mgmt-Service"
            Ensure = "Present"
            DependsOn = "[WindowsFeature]WebServerRole"
        }
        
    }
}