param($installPath, $toolsPath, $package, $project)
    $project.Object.References.Add("System.Xml")
	$project.Object.References.Add("System.Xml.Linq")