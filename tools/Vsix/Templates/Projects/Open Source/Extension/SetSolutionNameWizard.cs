using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TemplateWizard;
using EnvDTE;
using System.Windows;
using System.IO;
using System.Runtime.Remoting.Messaging;

namespace NetFx.Templates.Projects.OpenSource.Extension
{
	/// <summary>
	/// Adds the $solutionname$ dictionary replacement value, using the parent folder name.
	/// </summary>
	public class SetSolutionNameWizard : IWizard
	{
		public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
		{
			CallContext.SetData("$solutionname$", replacementsDictionary["$projectname$"]);
			CallContext.SetData("$safesolutionname$", replacementsDictionary["$safeprojectname$"]);
		}

		public void RunFinished()
		{
			CallContext.SetData("$solutionname$", null);
			CallContext.SetData("$safesolutionname$", null);
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
