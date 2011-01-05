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
		private Project project;

		public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
		{
		}

		public void RunFinished()
		{
			// Save solution with same name as the parent folder for this project.
			var parentDir = new System.IO.FileInfo(this.project.FullName).Directory.Parent;
			var solutionName = parentDir.Name + ".sln";

			project.DTE.Solution.SaveAs(Path.Combine(parentDir.FullName, solutionName));
		}

		public void BeforeOpeningFile(ProjectItem projectItem)
		{
		}

		public void ProjectFinishedGenerating(Project project)
		{
			this.project = project;
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
