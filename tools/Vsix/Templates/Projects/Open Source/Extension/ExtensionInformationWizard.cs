using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text.RegularExpressions;
using System.Windows;
using EnvDTE;
using Microsoft.VisualStudio.TemplateWizard;

namespace NetFx.Templates.Projects.OpenSource.Extension
{
    /// <summary>
    /// Prompts for and populates the extension information replacement dictionary.
    /// </summary>
    public class ExtensionInformationWizard : IWizard
    {
        /// <summary>
        /// Runs custom wizard logic at the beginning of a template wizard run.
        /// </summary>
        /// <param name="automationObject">The automation object being used by the template wizard.</param>
        /// <param name="replacementsDictionary">The list of standard parameters to be replaced.</param>
        /// <param name="runKind">A <see cref="T:Microsoft.VisualStudio.TemplateWizard.WizardRunKind"/> indicating the type of wizard run.</param>
        /// <param name="customParams">The custom parameters with which to perform parameter replacement in the project.</param>
        public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
        {
            //if (!new Microsoft.CSharp.CSharpCodeProvider().IsValidIdentifier(replacementsDictionary["$projectname$"]))
            //    throw new InvalidOperationException("Your chosen project name " + replacementsDictionary["$projectname$"] + " must be a valid C# code identifier.");

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

            var pathToRoot = Regex
                .Replace(targetDir.FullName, extensionsRoot.FullName.Replace("\\", "\\\\"), "", RegexOptions.IgnoreCase)
                .Split(Path.DirectorySeparatorChar)
                .Aggregate("..\\", (result, current) => result + "..\\");

            var ns = string.Join(".", Regex
                // We start from the parent directory, as we'll use the $safeprojectname$ to build the identifier later
                .Replace(targetDir.Parent.FullName, extensionsRoot.FullName.Replace("\\", "\\\\"), "", RegexOptions.IgnoreCase)
                .Split(new[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s))
                .Concat(new[] { replacementsDictionary["$projectname$"] }));

            var identifier = "NetFx." + ns;

            var view = new ExtensionInformationView();
            view.Model.Identifier = identifier;
            view.Model.Title = "NETFx " + ExtensionTitleSuggestion.Suggest(
                Regex.Replace(targetDir.Parent.FullName, extensionsRoot.FullName.Replace("\\", "\\\\"), "", RegexOptions.IgnoreCase),
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
                    CallContext.SetData("$" + property.Name + "$", property.GetValue(view.Model, null).ToString().XmlEncode());
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
