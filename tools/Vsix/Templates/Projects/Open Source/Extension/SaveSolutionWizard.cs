using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TemplateWizard;
using EnvDTE;
using System.Windows;
using System.IO;

namespace NetFx.Templates.Projects.OpenSource.Extension
{
	/// <summary>
	/// Saves the solution with the same name as the unfolded project parent directory, 
	/// under that directory.
	/// </summary>
	public class SaveSolutionWizard : IWizard
	{
		private DTE dte;
		private DirectoryInfo targetDir;
		private string projectName;

		public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
		{
			this.dte = automationObject as DTE;
			this.targetDir = new DirectoryInfo(replacementsDictionary["$destinationdirectory$"]);
			this.projectName = replacementsDictionary["$projectname$"];
		}

		public void RunFinished()
		{
			this.dte.Solution.SaveAs(Path.Combine(targetDir.FullName, this.projectName + ".sln"));

			// Delete if any, the old files.
			this.targetDir.Parent.GetFiles(projectName + ".*").ToList().ForEach(f => f.Delete());
		}

		public void BeforeOpeningFile(ProjectItem projectItem)
		{
		}

		public void ProjectFinishedGenerating(Project project)
		{
		}

		public void ProjectItemFinishedGenerating(ProjectItem projectItem)
		{
		}

		public bool ShouldAddProjectItem(string filePath)
		{
			return true;
		}
	}
}
