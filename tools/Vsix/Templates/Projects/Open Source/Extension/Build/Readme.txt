How to release a netfx extension:
1 - Create your source file under the Source project.
2 - Create as many unit tests you need under the Tests project, using xUnit for the tests if possible.
3 - Include the source file/s created in 1) as linked files in the $safeprojectname$\NuGet\content folder, and set their build action to Content.
4 - Update your package description in the Package.tt file.
5 - Build the "Build" project to get a new .nupkg ready for testing.
6 - Send pull request!