using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetFx.Templates.Projects.OpenSource.Extension
{
	public class ExtensionInformationModel
	{
		public ExtensionInformationModel()
		{
			this.Identifier = this.Title = this.Description = this.Authors =
				this.Tags = this.PathToRoot = this.TargetNamespace = string.Empty;
		}

		public string Identifier { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public string Authors { get; set; }
		public string Tags { get; set; }

		public string PathToRoot { get; set; }
		public string TargetNamespace { get; set; }
	}
}
