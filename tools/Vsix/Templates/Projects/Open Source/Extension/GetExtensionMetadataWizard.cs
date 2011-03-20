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
	/// Gets the $extensionid$ dictionary replacement value, using the target location path as well as the extension name.
	/// </summary>
	public class GetExtensionMetadataWizard : IWizard
	{
		public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
		{
			replacementsDictionary["$extensionid$"] = (string)CallContext.GetData("$extensionid$");
			replacementsDictionary["$extensiontitle$"] = (string)CallContext.GetData("$extensiontitle$");
			replacementsDictionary["$pathtoroot$"] = (string)CallContext.GetData("$pathtoroot$");
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
