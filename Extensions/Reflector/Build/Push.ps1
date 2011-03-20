$apiKey = Get-Content ..\..\..\apikey.txt
$packageFile = (Get-ChildItem -Filter *.nupkg -Path bin).Name
..\..\..\packages\NuGet.CommandLine.1.1.2113.118\Tools\NuGet.exe push -source http://packages.nuget.org/v1/ bin\$packageFile $apiKey

