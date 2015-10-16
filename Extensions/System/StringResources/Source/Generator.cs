using System;
using System.Collections.Generic;

namespace NetFx
{
	static class Generator
	{
		public static IGenerator Create (string language, string targetNamespace, string resourcesTypeName, string targetClassName, bool makePublic, ResourceArea rootArea)
		{
			var session = new Dictionary<string, object> {
				{ "ResourcesTypeName", resourcesTypeName },
				{ "TargetNamespace", targetNamespace },
				{ "TargetClassName", targetClassName },
				{ "MakePublic", makePublic },
				{ "RootArea", rootArea }
			};

			if ("C#".Equals (language, StringComparison.OrdinalIgnoreCase)) {
				var generator = new CsTypedResx {
					Session = session
				};

				generator.Initialize();

				return generator;
			}

			throw new NotSupportedException(string.Format("Language {0} is not supported yet.", language));
		}
	}

	interface IGenerator
	{
		string TransformText ();
	}

	partial class CsTypedResx : IGenerator
	{
	}
}
