%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe /p:Configuration=Release
pushd ..\..\..\packages\NuGet.CommandLine.1.0.11220.26\Tools
NuGet.exe pack ..\..\System\ComponentModel\ViewModel\Build\NuGet\Package.nuspec -BasePath ..\..\System\ComponentModel\ViewModel\Build\NuGet -OutputDirectory ..\..\System\ComponentModel\ViewModel\Build\bin
popd
