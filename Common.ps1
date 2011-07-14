# Builds all .slns recursively in Release mode, then 
# pushes all resulting .nupkg in bin\Release folders
# This function assumes it's called from the netfx root 
# folder, with the relative path being the path to 
# search for nugets.
function Push-Packages
{
	param([string] $relativePath)

	$apiKey = Get-Content apikey.txt

	Build-Packages $relativePath

	Write-Progress -Activity "Pushing NETFx" -Status "Publishing built packages recursively"

	Get-ChildItem -Path $relativePath -Recurse -Filter *.nupkg | Where-Object { $_.DirectoryName.EndsWith("bin\Release") }  | %{ Lib\NuGet.exe Push -Source http://packages.nuget.org/v1/ $_.FullName $apiKey }

	Write-Progress -Activity "Pushing NETFx" -Status "Done!" -Completed
}

# Builds all .slns recursively in Release mode, then 
# copies all resulting .nupkg in bin\Release folders
# to the Drop folder.
# This function assumes it's called from the netfx root 
# folder, with the relative path being the path to 
# search for nugets.
function Drop-Packages
{
	param([string] $relativePath)

	Build-Packages $relativePath

	mkdir Drop -ea silentlycontinue
	$dropDir = gci -Filter Drop
	Write-Host "Drop location is " $dropDir.FullName

	Get-ChildItem -Path $current -Recurse -Filter *.nupkg | `
	Where-Object { $_.DirectoryName.EndsWith("bin\Release") }  | %{ `
		$target = [System.IO.Path]::Combine($dropDir.FullName, $_.Name); `
		Remove-Item -Path $target -ea silentlycontinue; `
		$_.MoveTo($target); }

	Write-Progress -Activity "Deploying NETFx" -Status "Done!" -Completed
}

# Builds all .slns recursively in Release mode
# This function assumes it's called from the netfx root 
# folder, with the relative path being the path to 
# search for nugets.
function Build-Packages
{
	param([string] $relativePath)

	$msbuild = [System.Environment]::ExpandEnvironmentVariables("%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe")

	Write-Progress -Activity "Building NETFx" -Status "Building release recursively"

	# Build all extensions
	foreach ($build in (Get-ChildItem -Path $relativePath -Recurse -Filter *.sln))
	{
		pushd $build.DirectoryName

			$progress++
			Write-Progress -Activity "Building NETFx" -Status ("Cleaning " + $build.Name) -PercentComplete $progress
			&$msbuild /target:Clean /verbosity:quiet /p:Configuration=Debug | out-null
			&$msbuild /target:Clean /verbosity:quiet /p:Configuration=Release | out-null

			Write-Progress -Activity "Building NETFx" -Status ("Building " + $build.Name) -PercentComplete $progress
			&$msbuild /verbosity:quiet /p:Configuration=Release /nologo
		
		popd
	}

	Write-Progress -Activity "Building NETFx" -Status "Done!" -Completed
}