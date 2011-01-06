# Need XLinq to read the nuspec files to know where to copy them
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
        Write-Progress -Activity "Deploying NETFx" -Status ("Processing extension " + $build.Directory.Parent.Name) -PercentComplete $progress
        &$msbuild "/verbosity:quiet" | out-null
    
        cd bin\NuGet
        $spec = Get-ChildItem "Package.nuspec"
        $doc = [System.Xml.Linq.XDocument]::Load($spec.FullName)
        $id = $doc.Root.Element($ns + "metadata").Element($ns + "id").Value
    	$version = $doc.Root.Element($ns + "metadata").Element($ns + "version").Value
        $extensionDir = $dropDir.CreateSubdirectory($id)
        # $versionDir = $extensionDir.CreateSubdirectory($version)

        # Write-Progress -ParentId 1 -Activity "Copying package to drop location" -Status ("Copying from " + $spec.Directory + " to " + $versionDir)
        
        Copy-Item -Path $spec.Directory -Destination $extensionDir -Recurse
        $extensionDir.GetDirectories("NuGet")[0].MoveTo($extensionDir.FullName + "\" + $version)
    popd
}

# Copy all built packages to the Drops folder.
# $specs = Get-ChildItem Extensions -Recurse -Filter Package.nuspec | Where-Object { $_.DirectoryName.EndsWith("bin\NuGet") }

# foreach ($spec in $specs)
# {
#     $doc = [System.Xml.Linq.XDocument]::Load($spec.FullName)
#     $id = $doc.Root.Element($ns + "metadata").Element($ns + "id").Value
# 	$version = $doc.Root.Element($ns + "metadata").Element($ns + "version").Value
# 
#     $extensionDir = $dropDir.CreateSubdirectory($id)
#     Copy-Item -Path $spec.Directory -Destination $versionDir -Recurse
# }