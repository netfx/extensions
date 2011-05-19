param($installPath, $toolsPath, $package, $project)

	$project.Object.References.Add("Microsoft.VisualStudio.ComponentModelHost")
	$project.Object.References.Add("System.ComponentModel.Composition")