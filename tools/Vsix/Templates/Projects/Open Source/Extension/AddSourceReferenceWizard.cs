using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TemplateWizard;
using EnvDTE;
using System.Windows;
using System.IO;
using VSLangProj;

namespace NetFx.Templates.Projects.OpenSource.Extension
{
	/// <summary>
	/// Saves the solution with the same name as the unfolded project parent directory, 
	/// under that directory.
	/// </summary>
	public class AddSourceReferenceWizard : IWizard
	{
		private DTE dte;
		private VSProject targetProject;

		public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
		{
			this.dte = automationObject as DTE;
		}

		public void RunFinished()
		{
			if (this.dte != null && this.targetProject != null)
			{
				var sourceProject = this.dte.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name == "Source");
				if (sourceProject != null)
				{
					this.targetProject.References.AddProject(sourceProject);
				}
			}
		}

		public void BeforeOpeningFile(ProjectItem projectItem)
		{
		}

		public void ProjectFinishedGenerating(Project project)
		{
			this.targetProject = project.Object as VSProject;
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
