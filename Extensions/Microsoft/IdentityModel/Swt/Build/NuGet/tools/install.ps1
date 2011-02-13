param($installPath, $toolsPath, $package, $project)

	if ([system.reflection.assembly]::loadwithpartialname("Microsoft.IdentityModel") -eq $null)
	{
		write-warning Windows Identity Foundation is not installed. Opening browser to install from: http://support.microsoft.com/?kbid=974405
		start http://support.microsoft.com/?kbid=974405
		uninstall-package netfx-Microsoft.IdentityModel.Swt
		return
	}

	# Set the issuer registry
	$web = $project.ProjectItems | Where-Object { $_.Name -eq "Web.config" };

	if ($web -ne $null)
	{
		$xml = new-object System.Xml.XmlDocument
		$xml.Load($web.FileNames(1))
		$registry = $xml.SelectSingleNode("configuration/microsoft.identityModel/service/issuerNameRegistry")

		if ($registry -eq $null)
		{
			write-warning "Please add an STS reference to your project before installing SWT support. Reinstall again after doing so."
			uninstall-package netfx-Microsoft.IdentityModel.Swt
		}
		else
		{
			$registry.type = "Microsoft.IdentityModel.Swt.SwtIssuerNameRegistry"
			$xml.Save($web.FileNames(1))
		}
	}
	else
	{
		write-warning "Web.config file not found! The project must be a web application."
		uninstall-package netfx-Microsoft.IdentityModel.Swt
	}
	
	$project.Object.References.Add("System.Configuration")
	$project.Object.References.Add("System.IdentityModel")
	$project.Object.References.Add("System.ServiceModel")
	$project.Object.References.Add("System.ServiceModel.Web")
	$project.Object.References.Add("System.Web")
	$project.Object.References.Add("System.Xml")
	$project.Object.References.Add("Microsoft.IdentityModel")

	write-warning "IMPORTANT: Please set the value of SwtSigningKey in your Web.config appSettings section."