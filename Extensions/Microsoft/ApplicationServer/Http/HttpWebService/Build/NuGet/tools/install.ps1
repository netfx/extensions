param($installPath, $toolsPath, $package, $project)
	write-warning This package has been replaced with netfx-WebApi.Testing. Installing the right one now.
	uninstall-package netfx-Microsoft.ApplicationServer.Http.HttpWebService -ProjectName $project.Name
	install-package netfx-WebApi.Testing -ProjectName $project.Name
