%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe /p:Configuration=Release /verbosity:minimal
pushd ..\..\..\..\..\packages\NuGet.CommandLine.1.0.11220.26\Tools
NuGet.exe pack ..\..\..\Extensions\System\Xml\XmlSerializer\Build\bin\NuGet\Package.nuspec -BasePath ..\..\..\Extensions\System\Xml\XmlSerializer\Build\bin\NuGet -OutputDirectory ..\..\..\Extensions\System\Xml\XmlSerializer\Build\bin
popd
