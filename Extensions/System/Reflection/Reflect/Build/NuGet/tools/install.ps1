param($installPath, $toolsPath, $package, $project)
	write-host This package ID is obsolete. Replacing with netfx-Reflector
	uninstall-package netfx-System.Reflection.Reflect -ProjectName $project.Name
	install-package netfx-Reflector -ProjectName $project.Name
