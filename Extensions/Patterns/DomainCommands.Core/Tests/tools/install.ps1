param($installPath, $toolsPath, $package, $project)
	# Not done as a dependency because the public xUnit
	# release doesn't have the TD.NET runner integration 
	# we use and therefore link directly from our Lib
	if (($project.Object.References | Where-Object { $_.Name -eq 'xunit' } | Measure-Object).Count -eq 0)
	{
		install-package xunit
	}