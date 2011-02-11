%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe /p:Configuration=Release /verbosity:minimal
pushd ..\..\..\..\..\packages\NuGet.CommandLine.1.1.2113.118\Tools
NuGet.exe pack ..\..\..\Extensions\Microsoft\IdentityModel\Swt\Build\bin\NuGet\Package.nuspec -BasePath ..\..\..\Extensions\Microsoft\IdentityModel\Swt\Build\bin\NuGet -OutputDirectory ..\..\..\Extensions\Microsoft\IdentityModel\Swt\Build\bin
popd
