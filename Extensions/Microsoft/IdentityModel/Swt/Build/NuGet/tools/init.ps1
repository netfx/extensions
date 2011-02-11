param($installPath, $toolsPath, $package, $project)

	# For the case where WIF is installed and VS restarted.
	$project.Object.References.Add("Microsoft.IdentityModel")
