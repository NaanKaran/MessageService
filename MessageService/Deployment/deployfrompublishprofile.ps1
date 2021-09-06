#Usage: powershell .\deployWebApp.ps1 [pathToWebAppPackageFolder] [webAppPackageZipFile] [pathToPublishSettingsFile] [configuration]
param([string]$packageFolderPath,[string]$packageZip,[string]$publishProfilePath,[string]$configuration)

#MsDeploy.exe location (if not in path)
[string]$msDeployExe="C:\Program Files (x86)\IIS\Microsoft Web Deploy V3\msdeploy.exe"

#Get publish settings from file
[xml]$xml=Get-Content($publishProfilePath)
[string]$azureSite=$xml.publishData.publishProfile.msDeploySite.get(0)
[string]$azureUrl=$xml.publishData.publishProfile.publishUrl.get(0)
[string]$azureUsername=$xml.publishData.publishProfile.userName.get(0)
[string]$azurePassword=$xml.publishData.publishProfile.userPWD.get(0)

#MsDeploy "white label" webapp package (additional configs to follow)
$msDeployPackageCommand = "`"$msDeployExe `" -verb:sync  -source:package=`"$packageFolderPath\$packageZip`" -dest:auto,ComputerName=`"https://$azureUrl/msdeploy.axd?site=$azureSite`",UserName=`"$azureUsername`",Password=`"$azurePassword`",IncludeAcls=`"False`",AuthType=`"Basic`" -setParam:name=`"IIS Web Application Name`",value=`"$azureSite`" -disableLink:AppPoolExtension -disableLink:ContentExtension -disableLink:CertificateExtension -retryAttempts=2 -enableRule:AppOffline -verbose -userAgent=`"VS12.0:PublishDialog:WTE12.3.51016.0`"";

Write-Output ("MsDeploy Command: " + $msDeployPackageCommand);
cmd.exe /c "$msDeployPackageCommand";

#$filePath = $packageFolderPath + "\" + $packageZip
#C:\Program Files (x86)\IIS\Microsoft Web Deploy V3\msdeploy.exe -verb:sync -source:package=$filePath -dest:auto,ComputerName='https://$azureUrl/msdeploy.axd?site=$azureSite',UserName=$azureUsername,Password=$azurePassword,AuthType='Basic' -setParam:name='IIS Web Application Name',value=$azureSite        