﻿param($installPath, $toolsPath, $package, $project)
	write-host This package has been replaced with netfx-WebApi.Testing. The right package has been installed as a dependency.
	write-host Uninstalling this package now.
	uninstall-package netfx-Microsoft.ApplicationServer.Http.HttpWebService -ProjectName $project.Name
