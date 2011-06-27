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
	/// Prompts for and populates the extension information replacement dictionary.
	/// </summary>
	public class ExtensionInformationWizard : IWizard
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
			{
				targetDir.Delete(true);
				Backout(string.Format(
					"Selected target path '{0}' is not located under the root NETFx Extensions repository folder.", targetDir));
			}

			var pathToRoot = targetDir.FullName
				.Replace(extensionsRoot.FullName, "")
				.Split(Path.DirectorySeparatorChar)
				.Aggregate("..\\", (result, current) => result + "..\\");

			var ns = string.Join(".", targetDir.Parent.FullName
				// We start from the parent directory, as we'll use the $safeprojectname$ to build the identifier later
				.Replace(extensionsRoot.FullName, "")
				.Split(new[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries)
				.Concat(new[] { replacementsDictionary["$projectname$"] }));
			var identifier = "netfx-" + ns;

			var view = new ExtensionInformationView();
			view.Model.Identifier = identifier;
			view.Model.Title = "NETFx " + ExtensionTitleSuggestion.Suggest(
				targetDir.Parent.FullName.Replace(extensionsRoot.FullName, ""),
				replacementsDictionary["$projectname$"]);
			view.Model.PathToRoot = pathToRoot;
			view.Model.TargetNamespace = ns.Substring(0, ns.LastIndexOf('.'));
			view.Model.Authors = replacementsDictionary["$username$"] + ", Clarius";
			view.Model.Tags = "netfx foo bar";
			view.Owner = Application.Current.MainWindow;

			if (view.ShowDialog().GetValueOrDefault())
			{
				foreach (var property in typeof(ExtensionInformationModel).GetProperties())
				{
					CallContext.SetData("$" + property.Name + "$", property.GetValue(view.Model, null));
				}
			}
			else
			{
				targetDir.Delete(true);
				throw new WizardBackoutException();
			}
		}

		private void Backout(string message)
		{
			MessageBox.Show(message, "NETFx Extension", MessageBoxButton.OK, MessageBoxImage.Stop);
			throw new WizardBackoutException(message);
		}

		public void RunFinished()
		{
			foreach (var property in typeof(ExtensionInformationModel).GetProperties())
			{
				CallContext.SetData("$" + property.Name + "$", null);
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
