param($installPath, $toolsPath, $package, $project)

function global:Update-Packages 
{
	foreach ($project in Get-ProjectNames)
	{
		foreach ($package in (Get-Package -Project $project))
		{
			Update-Package $package.Id -Project $project
		}
	}
}