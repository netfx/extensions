# Need XLinq to read the nuspec files
[System.Reflection.Assembly]::LoadWithPartialName("System.Xml.Linq") | out-null
$ns = [System.Xml.Linq.XNamespace]::Get("http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd")

# Locate the nuget command line
$nuget = Lib\NuGet.exe
$msbuild = [System.Environment]::ExpandEnvironmentVariables("%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe")

Write-Progress -Activity "Deploying NETFx" -Status "Cleaning drop location"
# Remove-Item -Path Drop -Force -Recurse -ErrorAction SilentlyContinue | out-null
Remove-Item Drop -Recurse -ErrorAction SilentlyContinue | out-null
$dropDir = [System.IO.Directory]::CreateDirectory("Drop") 

# Build all extensions
foreach ($build in (Get-ChildItem Extensions -Recurse -Filter *.sln))
{
    pushd $build.DirectoryName

        $progress++
        Write-Progress -Activity "Cleaning " -Status ("Cleaning extension " + $build.Directory.Parent.Name) -PercentComplete $progress
        &$msbuild /target:Clean /verbosity:quiet /p:Configuration=Debug | out-null
        &$msbuild /target:Clean /verbosity:quiet /p:Configuration=Release | out-null

        Write-Progress -Activity "Building " -Status ("Building extension " + $build.Directory.Parent.Name) -PercentComplete $progress
        Write-Progress -Activity "Deploying NETFx" -Status ("Building extension " + $build.Directory.Parent.Name) -PercentComplete $progress
        &$msbuild /verbosity:quiet /p:Configuration=Release
   
    
		# At this point there should only be one nupkg
		gci -filter *.nupkg -recurse | Where-Object { $_.directoryname.endswith("bin\Release") }  | %{ write-host $_.MoveTo("$dropDir.FullName\\$_") }
         
    popd
}

Write-Progress -Activity "Deploying NETFx" -Status "Completed" -Completed $true
