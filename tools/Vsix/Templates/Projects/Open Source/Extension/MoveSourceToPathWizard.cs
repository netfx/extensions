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
	/// Moves the unfolded Source.cs file to the target namespace folder area 
	/// under content.
	/// </summary>
	public class MoveSourceToPathWizard : IWizard
	{
		private DTE dte;
		private Project project;
		private string extensionNamespace;

		public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
		{
			this.dte = automationObject as DTE;
			this.extensionNamespace = replacementsDictionary[Replacement.Key(x => x.TargetNamespace)];
		}

		public void RunFinished()
		{
			if (this.dte != null)
			{
				var sourceItem = this.project.ProjectItems
					.OfType<ProjectItem>()
					.FirstOrDefault(item =>
						item.Kind == Constants.vsProjectItemKindPhysicalFile &&
						item.Name.EndsWith(".cs") && 
						!item.Name.StartsWith("AssemblyInfo"));

				if (sourceItem != null)
				{
					// Locate the content folder in the source project.
					var targetFolder = this.project.ProjectItems
						.OfType<ProjectItem>()
						.First(item =>
							item.Kind == Constants.vsProjectItemKindPhysicalFolder &&
							item.Name == "content")
						.ProjectItems
						.OfType<ProjectItem>()
						.First(item =>
							item.Kind == Constants.vsProjectItemKindPhysicalFolder &&
							item.Name == "netfx");

					if (targetFolder != null)
					{
						// Build all folders that contain the extension path.
						foreach (var path in this.extensionNamespace.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries))
						{
							targetFolder = targetFolder.ProjectItems.AddFolder(path);
						}

						var sourceFile = sourceItem.get_FileNames(1);
						targetFolder.ProjectItems.AddFromFileCopy(sourceFile);
						sourceItem.Delete();
					}
				}
			}
		}

		public void BeforeOpeningFile(ProjectItem projectItem)
		{
		}

		public void ProjectFinishedGenerating(Project project)
		{
			this.project = project;
		}

		public void ProjectItemFinishedGenerating(ProjectItem item)
		{
		}

		public bool ShouldAddProjectItem(string filePath)
		{
			return true;
		}

		private IEnumerable<DirectoryInfo> TraverseUp(DirectoryInfo dir)
		{
			var current = dir;
			while (current != null)
			{
				yield return current;
				current = current.Parent;
			}
		}
	}
}