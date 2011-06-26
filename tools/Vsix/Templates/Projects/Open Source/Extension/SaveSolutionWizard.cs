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
			var sln = Path.Combine(targetDir.FullName, this.projectName + ".sln");
			this.dte.Solution.SaveAs(sln);

			// Delete if any, the old files.
			this.targetDir.Parent.GetFiles(projectName + ".*").ToList().ForEach(f => f.Delete());

			// Close and reopen to cause all project to refresh links and what-not.
			foreach (var project in this.dte.Solution.Projects.OfType<Project>())
			{
				project.SaveAs(project.FullName);
			}

			this.dte.Solution.SaveAs(sln);
			this.dte.Solution.Close(true);
			this.dte.Solution.Open(sln);
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
