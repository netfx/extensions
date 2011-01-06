%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe /p:Configuration=Release /verbosity:minimal
pushd ..\..\..\..\..\..\..\packages\NuGet.CommandLine.1.0.11220.26\Tools
NuGet.exe pack ..\..\..\tools\ContributorVsix\Templates\Projects\Open Source\Extension\Build\bin\NuGet\Package.nuspec -BasePath ..\..\..\tools\ContributorVsix\Templates\Projects\Open Source\Extension\Build\bin\NuGet -OutputDirectory ..\..\..\tools\ContributorVsix\Templates\Projects\Open Source\Extension\Build\bin
popd
