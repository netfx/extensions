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
	/// Adds the $extensionid$ dictionary replacement value, using the target location path as well as the extension name.
	/// </summary>
	public class SetExtensionMetadataWizard : IWizard
	{
		public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
		{
			if (!new Microsoft.CSharp.CSharpCodeProvider().IsValidIdentifier(replacementsDictionary["$projectname$"]))
				throw new InvalidOperationException("Your chosen project name " + replacementsDictionary["$projectname$"] + " must be a valid C# code identifier.");

			var targetDir = new DirectoryInfo(replacementsDictionary["$destinationdirectory$"]);
			var extensionsRoot = (from parentDir in TraverseUp(targetDir)
								  // Lookup the root of the netfx repository
								  where parentDir.EnumerateFileSystemInfos("netfx.txt").Any()
								  // Then move down to the Extensions directory where all extensions live
								  select parentDir.EnumerateDirectories("Extensions").FirstOrDefault())
								 .FirstOrDefault();

			if (extensionsRoot == null)
				throw new InvalidOperationException(string.Format(
					"Selected target path '{0}' is not located under the root NETFx Extensions repository folder.", targetDir));

			var identifier = "netfx-" + string.Join(".", targetDir.Parent.FullName
				// We start from the parent directory, as we'll use the $safeprojectname$ to build the identifier later
				.Replace(extensionsRoot.FullName, "")
				.Split(new[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries)
				.Concat(new[] { replacementsDictionary["$projectname$"] }));

			CallContext.SetData("$extensionid$", identifier);
			CallContext.SetData("$extensiontitle$", ExtensionTitleSuggestion.Suggest(
				targetDir.Parent.FullName.Replace(extensionsRoot.FullName, ""),
				replacementsDictionary["$projectname$"]));
		}

		public void RunFinished()
		{
			CallContext.SetData("$extensionid$", null);
			CallContext.SetData("$extensiontitle$", null);
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
