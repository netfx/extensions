# Need XLinq to read the nuspec files to know where to copy them
# Locate the nuget command line
$nuget = Get-ChildItem -Filter NuGet.exe -Recurse

$dropDir = [System.IO.Directory]::CreateDirectory("Drop") 

# Build all extensions
foreach ($package in (Get-ChildItem Drop -Filter *.nupkg))
{
	$progress++

	Write-Progress -Activity "Publishing NETFx" -Status ("uploading package " + $package.Name) -PercentComplete $progress
	&($nuget.FullName) "push" "-source" $package.FullName cf09128a-423d-4bf8-9bc0-414fae8766c8 | out-null
}

Write-Progress -Activity "Publishing NETFx" -Status "Completed" -Completed $true
