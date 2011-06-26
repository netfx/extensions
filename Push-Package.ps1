function Push-Package
{
	param([string] $relativePath)

	if (((Get-ChildItem -Filter *.nupkg -Path $relativePath -Recurse) | Measure-Object).Count -gt 1)
	{
		throw new-object system.invalidoperationexception "Multiple .nupkg files found under '$relativePath'. Make sure there's only one to push."
	}

	$packageFile = (Get-ChildItem -Filter *.nupkg -Path $relativePath -Recurse).FullName
	$apiKey = Get-Content apikey.txt
	Lib\NuGet.exe Push -source http://packages.nuget.org/v1/ "$packageFile" $apiKey
}