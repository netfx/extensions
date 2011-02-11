param($installPath, $toolsPath, $package, $project)

	$project.Object.References.Add("System.Configuration")
	$project.Object.References.Add("System.IdentityModel")
	$project.Object.References.Add("System.ServiceModel")
	$project.Object.References.Add("System.ServiceModel.Web")
	$project.Object.References.Add("System.Web")
	$project.Object.References.Add("System.Xml")

	if ([system.reflection.assembly]::loadwithpartialname("Microsoft.IdentityModel") -eq $null)
	{
		write-warning Windows Identity Foundation is not installed. Opening browser to install from: http://support.microsoft.com/?kbid=974405
		write-warning Add a reference to Microsoft.IdentityModel afterwards. Otherwise, when the solution is reopened, it will be done automatically.
		start http://support.microsoft.com/?kbid=974405
	}
	else
	{
		$project.Object.References.Add("Microsoft.IdentityModel")
	}

	# Set the issuer registry
	$web = $project.ProjectItems | Where-Object { $_.Name -eq "Web.config" };
	if ($web -ne $null)
	{
		$xml = new-object System.Xml.XmlDocument
		$xml.Load($web.FileNames(0))
		$registry = $xml.SelectSingleNode("configuration/microsoft.identityModel/service/issuerNameRegistry")

		if ($registry -eq $null)
		{
			write-error "Please add an STS reference to your project before installing SWT support. Uninstall this package and reinstall again after doing so."
		}

		$registry.type = "Microsoft.IdentityModel.Swt.SwtIssuerNameRegistry"
		$xml.Save($web.FileNames(0))
	}
	else
	{
		write-error "Web.config file not found!"
	}

	write-warning "IMPORTANT: Please set the value of SwtSigningKey in your Web.config appSettings section."