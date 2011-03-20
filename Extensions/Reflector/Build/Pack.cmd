pushd ..\..\..\packages\NuGet.CommandLine.1.1.2113.118\Tools
NuGet.exe pack ..\..\..\Extensions\Reflector\Build\bin\NuGet\Package.nuspec -BasePath ..\..\..\Extensions\Reflector\Build\bin\NuGet -OutputDirectory ..\..\..\Extensions\Reflector\Build\bin
popd
