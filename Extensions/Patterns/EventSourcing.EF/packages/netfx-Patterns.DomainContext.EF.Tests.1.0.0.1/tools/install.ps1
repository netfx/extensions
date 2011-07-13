param($installPath, $toolsPath, $package, $project)
	# Not done as a dependency because we don't want to have myriad 
	# versions of xUnit in netfx source itself, and xUnit public 
	# release doesn't have the TD.NET runner integration we use.
	if (($project.Object.References | Where-Object { $_.Name -eq 'xunit' } | Measure-Object).Count -eq 0)
	{
		install-package xunit
	}