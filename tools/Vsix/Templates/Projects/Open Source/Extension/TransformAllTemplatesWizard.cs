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
	public class TransformAllTemplatesWizard : IWizard
	{
		private DTE dte;

		public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
		{
			this.dte = automationObject as DTE;
		}

		public void RunFinished()
		{
			if (this.dte != null)
			{
				this.dte.ExecuteCommand("TextTransformation.TransformAllTemplates");
			}
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
