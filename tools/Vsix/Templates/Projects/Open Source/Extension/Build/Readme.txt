How to release a netfx extension:
1 - Create your source file under the Source project.
2 - Create as many unit tests you need under the Tests project, using xUnit for the tests if possible.
3 - Include the source file/s created in 1) as linked files in the $safeprojectname$\NuGet\content folder, and set their build action to Content.
4 - Update your package description in the Package.nuspec file.
5 - Run the Release.cmd batch file from a command line, and test your package in a new blank solution.
6 - Send pull request!