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

function Push-Packages
{
	param([string] $relativePath)

	# Locate the nuget command line
	$nuget = Lib\NuGet.exe
	$apiKey = Get-Content apikey.txt
	$msbuild = [System.Environment]::ExpandEnvironmentVariables("%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe")

	Write-Progress -Activity "Pushing NETFx" -Status "Building release recursively"

	# Build all extensions
	foreach ($build in (Get-ChildItem -Path $relativePath -Recurse -Filter *.sln))
	{
		pushd $build.DirectoryName

			$progress++
			Write-Progress -Activity "Cleaning " -Status ("Cleaning extension " + $build.Directory.Parent.Name) -PercentComplete $progress
			&$msbuild /target:Clean /verbosity:quiet /p:Configuration=Debug | out-null
			&$msbuild /target:Clean /verbosity:quiet /p:Configuration=Release | out-null

			Write-Progress -Activity "Building " -Status ("Building extension " + $build.Directory.Parent.Name) -PercentComplete $progress
			&$msbuild /verbosity:quiet /p:Configuration=Release /nologo
		
		popd
	}

	Write-Progress -Activity "Pushing NETFx" -Status "Publishing built packages recursively"

	Get-ChildItem -Path $relativePath -Recurse -Filter *.nupkg | Where-Object { $_.DirectoryName.EndsWith("bin\Release") }  | %{ &$nuget Push -Source http://packages.nuget.org/v1/ $_.FullName $apiKey }
}