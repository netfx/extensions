using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity.Design.PluralizationServices;

namespace System.Net.Http
{
	/// <summary>
	/// Entity resource convention that pluralizes singular-named 
	/// entity types to make the resource name. If the entity type 
	/// is already a plural name, returns it as-is.
	/// </summary>
	/// <nuget id="netfx-System.Net.Http.HttpEntityConventionClient" />
	internal class PluralizerResourceConvention : IEntityResourceNameConvention
	{
		private PluralizationService pluralizer;
		private PluralizerResourceFormat format;

		/// <summary>
		/// Initializes the convention using the default en-US culture and 
		/// <see cref="PluralizerResourceFormat.CamelCase"/> for pluralized names.
		/// </summary>
		public PluralizerResourceConvention()
			: this(PluralizationService.CreateService(new System.Globalization.CultureInfo("en-US")), PluralizerResourceFormat.CamelCase)
		{
		}

		/// <summary>
		/// Initializes the convention with a custom pluralizer and resource format.
		/// </summary>
		public PluralizerResourceConvention(PluralizationService pluralizer, PluralizerResourceFormat resourceFormat)
		{
			this.pluralizer = pluralizer;
			this.format = resourceFormat;
		}

		/// <summary>
		/// Gets the name of the resource corresponding to the
		/// given entity by pluralizing it if necessary.
		/// </summary>
		public string GetResourceName(Type entityType)
		{
			var resourceName = this.pluralizer.IsSingular(entityType.Name) ?
				this.pluralizer.Pluralize(entityType.Name) :
				entityType.Name;

			switch (this.format)
			{
				case PluralizerResourceFormat.PascalCase:
					return Char.IsUpper(resourceName[0]) ?
						resourceName : new string(new[] { Char.ToUpper(resourceName[0]) }.Concat(resourceName.Skip(1)).ToArray());
				case PluralizerResourceFormat.CamelCase:
					return Char.IsLower(resourceName[0]) ?
						resourceName : new string(new[] { Char.ToLower(resourceName[0]) }.Concat(resourceName.Skip(1)).ToArray());
				case PluralizerResourceFormat.LowerCase:
					return resourceName.ToLower();
				default:
					throw new NotSupportedException();
			}
		}
	}
}