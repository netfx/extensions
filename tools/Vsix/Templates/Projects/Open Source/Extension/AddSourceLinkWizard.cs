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
	public class AddSourceLinkWizard : IWizard
	{
		private DTE dte;
		private Project project;

		public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
		{
			this.dte = automationObject as DTE;
		}

		public void RunFinished()
		{
			if (this.dte != null)
			{
				var sourceItem = (from project in this.dte.Solution.Projects.OfType<Project>()
								  where project.Name == "Source"
								  from item in project.ProjectItems.OfType<ProjectItem>()
								  where item.Kind == Constants.vsProjectItemKindPhysicalFile
								  select item)
								 .FirstOrDefault();

				if (sourceItem != null)
				{
					var extensionAreaDir = new FileInfo(this.project.FullName)
						// Directory where the Build.csproj lives
						.Directory
						// Parent directory, i.e. XmlSerializer
						.Parent
						// Parent directory, i.e. Xml (or the root Extensions if not categorized)
						.Parent;

					var extensionsRoot = (from parentDir in TraverseUp(extensionAreaDir)
										  // Lookup the root of the netfx repository
										  where parentDir.EnumerateFileSystemInfos("netfx.txt").Any()
										  // Then move down to the Extensions directory where all extensions live
										  select parentDir.EnumerateDirectories("Extensions").FirstOrDefault())
										 .FirstOrDefault();

					// Locate the NuGet\content\netfx folder in the build project.
					var targetFolder = (from nugetFolder in this.project.ProjectItems.OfType<ProjectItem>()
										where nugetFolder.Kind == Constants.vsProjectItemKindPhysicalFolder && nugetFolder.Name == "NuGet"
										from contentFolder in nugetFolder.ProjectItems.OfType<ProjectItem>()
										where contentFolder.Kind == Constants.vsProjectItemKindPhysicalFolder && contentFolder.Name == "content"
										from netfxFolder in contentFolder.ProjectItems.OfType<ProjectItem>()
										where netfxFolder.Kind == Constants.vsProjectItemKindPhysicalFolder && netfxFolder.Name == "netfx"
										select netfxFolder)
										.FirstOrDefault();

					if (extensionsRoot != null && targetFolder != null)
					{
						// Build all folders that contain the extension path.
						var categoryPath = extensionAreaDir.FullName.Replace(extensionsRoot.FullName, "");
						if (categoryPath.Length != 0)
						{
							foreach (var path in categoryPath.Split(new[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries))
							{
								targetFolder = targetFolder.ProjectItems.AddFolder(path);
							}
						}

						var item = targetFolder.ProjectItems.AddFromFile(sourceItem.FileNames[1]);
						item.Properties.Item("ItemType").Value = "Content";
						item.Properties.Item("CopyToOutputDirectory").Value = 1;
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

		public void ProjectItemFinishedGenerating(ProjectItem projectItem)
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