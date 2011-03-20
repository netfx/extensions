param($installPath, $toolsPath, $package, $project)
	write-host This package ID is obsolete. Replacing with netfx-Guard
	uninstall-package Guard -ProjectName $project.Name
	install-package netfx-Guard -ProjectName $project.Name
