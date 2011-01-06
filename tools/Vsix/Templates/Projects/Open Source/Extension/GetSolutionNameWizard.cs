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
	/// Adds the $solutionname$ dictionary replacement value, taking it from the current call context.
	/// </summary>
	public class GetSolutionNameWizard : IWizard
	{
		public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
		{
			replacementsDictionary["$solutionname$"] = (string)CallContext.GetData("$solutionname$");
			replacementsDictionary["$safesolutionname$"] = (string)CallContext.GetData("$solutionname$");
		}

		public void RunFinished()
		{
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
