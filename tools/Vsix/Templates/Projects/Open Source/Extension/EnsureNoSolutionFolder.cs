using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TemplateWizard;
using EnvDTE;
using System.Windows;
using System.IO;
using Microsoft.VisualStudio.Shell;

namespace NetFx.Templates.Projects.OpenSource.Extension
{
	/// <summary>
	///	Ensures that the "Create directory for solution" flag is not checked.
	/// </summary>
	public class EnsureNoSolutionFolder : IWizard
	{
		public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
		{
			var destinationDirectory = replacementsDictionary["$destinationdirectory$"];

			var paths = destinationDirectory.Split(Path.DirectorySeparatorChar).Reverse().ToList();
			if (paths[0].Equals(paths[1]))
				throw new WizardBackoutException();
		}

		public void RunFinished()
		{
		}

		public void BeforeOpeningFile(EnvDTE.ProjectItem projectItem)
		{
		}

		public void ProjectFinishedGenerating(EnvDTE.Project project)
		{
		}

		public void ProjectItemFinishedGenerating(EnvDTE.ProjectItem projectItem)
		{
		}

		public bool ShouldAddProjectItem(string filePath)
		{
			return true;
		}
	}
}
