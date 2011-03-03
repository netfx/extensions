# Locate the nuget command line
$nuget = Get-ChildItem -Filter NuGet.exe -Recurse

$dropDir = [System.IO.Directory]::CreateDirectory("Drop") 
$apiKey = $apiKey = Get-Content apikey.txt

# Push all extensions
foreach ($package in (Get-ChildItem Drop -Filter *.nupkg))
{
	$progress++

	Write-Progress -Activity "Publishing NETFx" -Status ("uploading package " + $package.Name) -PercentComplete $progress
	&($nuget.FullName) "push" "-source" $package.FullName $apiKey
}

Write-Progress -Activity "Publishing NETFx" -Status "Completed" -Completed $true
