How the NETFx project works
---------------------------
The C# Source and Tests projects have the same layout 
that the corresponding NuGet package will have: everything 
under "content" folder will go straight to the package 
output, as will "tools" and even "lib" if create such 
a folder.

Building the NETFx extension
----------------------------
When building the extension in Debug mode, you will notice 
that the generated package version number is incremented with 
every build. This is intended and helps testing and refining 
your package locally before publishing (NuGet client aggressive 
profile-wide caching makes this harder).

At the same time, switching to the Release build configuration 
causes the exact version number specified in the Package.nuspec 
to be used by the generated pacakge.

So in order to publish an update, you must manually increment 
the Package.nuspec version number to the desired value.

Publishing the NETFx extension
------------------------------
Open a command prompt and navigate to the bin\Release 
(or Debug) output folder for the project to publish. Enter
the following command:
   powershell .\push.ps1

Note that at the root of the NETFx source tree, there must be 
a text file called 'apikey.txt' with your nuget.org API key, 
and you must be an owner of the project you're contributing.
