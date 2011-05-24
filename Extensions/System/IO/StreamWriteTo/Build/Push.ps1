
$apiKey = Get-Content ..\..\..\..\..\apikey.txt
$packageFile = (Get-ChildItem -Filter *.nupkg -Path bin).Name
..\..\..\..\..\NuGet.exe push -source http://packages.nuget.org/v1/ bin\$packageFile $apiKey
