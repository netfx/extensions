# Need XLinq to read the nuspec files
[System.Reflection.Assembly]::LoadWithPartialName("System.Xml.Linq") | out-null
$ns = [System.Xml.Linq.XNamespace]::Get("http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd")

# Locate the nuget command line
$nuget = Get-ChildItem -Filter NuGet.exe -Recurse
$msbuild = [System.Environment]::ExpandEnvironmentVariables("%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe")

Write-Progress -Activity "Deploying NETFx" -Status "Cleaning drop location"
Remove-Item -Path Drop -Force -Recurse -ErrorAction SilentlyContinue | out-null
Remove-Item Drop -Recurse -ErrorAction SilentlyContinue | out-null
$dropDir = [System.IO.Directory]::CreateDirectory("Drop") 

# Build all extensions
foreach ($build in (Get-ChildItem Extensions -Recurse -Filter Build.csproj))
{
    pushd $build.DirectoryName

        $progress++
        Write-Progress -Activity "Deploying NETFx" -Status ("Building extension " + $build.Directory.Parent.Name) -PercentComplete $progress
        &$msbuild "/verbosity:quiet" | out-null
    
        cd bin\NuGet
        $spec = Get-ChildItem "Package.nuspec"
        $doc = [System.Xml.Linq.XDocument]::Load($spec.FullName)
        $id = $doc.Root.Element($ns + "metadata").Element($ns + "id").Value

        Write-Progress -Activity "Deploying NETFx" -Status ("Compiling package " + $build.Directory.Parent.Name + "(id: " + $id + ")") -PercentComplete $progress
        &($nuget.FullName) "pack" $spec.FullName -BasePath $spec.Directory.FullName -OutputDirectory $dropDir.FullName | out-null
        
    popd
}

Write-Progress -Activity "Deploying NETFx" -Status "Completed" -Completed $true
