param($installPath, $toolsPath, $package, $project)
	$project.DTE.Solution.FindProjectItem("Reflect.Overloads.tt").Object.RunCustomTool()
