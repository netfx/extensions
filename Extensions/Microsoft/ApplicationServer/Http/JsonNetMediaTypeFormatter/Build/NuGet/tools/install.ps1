param($installPath, $toolsPath, $package, $project)
	write-warning This package has been replaced with netfx-WebApi.JsonNetFormatter. Installing the right one now.
	uninstall-package netfx-Microsoft.ApplicationServer.Http.JsonNetMediaTypeFormatter -ProjectName $project.Name
	install-package netfx-WebApi.JsonNetFormatter -ProjectName $project.Name
