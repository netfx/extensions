%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe /p:Configuration=Release /verbosity:minimal
pushd ..\..\..\..\..\..\packages\NuGet.CommandLine.1.1.2113.118\Tools
NuGet.exe pack ..\..\..\Extensions\System\Windows\Input\DelegateCommand\Build\bin\NuGet\Package.nuspec -BasePath ..\..\..\Extensions\System\Windows\Input\DelegateCommand\Build\bin\NuGet -OutputDirectory ..\..\..\Extensions\System\Windows\Input\DelegateCommand\Build\bin
popd
