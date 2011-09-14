using System;
using System.ComponentModel;
using System.Globalization;
using System.Resources;

namespace System.ComponentModel
{
	/// <summary>
	/// An attribute that provides a localization from a resource file.
	/// </summary>
	[Serializable]
	[AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
	public sealed class CategoryResourceAttribute : CategoryAttribute
	{
		private string localizedString;

		/// <summary>
		/// Initializes a new instance of the <see cref="CategoryResourceAttribute"/> class.
		/// </summary>
		/// <param name="resourceName">The resource name.</param>
		/// <param name="resourceType">The type of the resource.</param>
		public CategoryResourceAttribute(string resourceName, Type resourceType)
			: base(resourceName)
		{
			Guard.NotNull(() => resourceName, resourceName);
			Guard.NotNull(() => resourceType, resourceType);

			this.ResourceName = resourceName;
			this.ResourceType = resourceType;
		}

		/// <summary>
		/// Gets the name of the resource.
		/// </summary>
		public string ResourceName { get; private set; }

		/// <summary>
		/// Gets the type of the resource.
		/// </summary>
		public Type ResourceType { get; private set; }

		/// <summary>
		/// Returns the localized string from the resource file.
		/// </summary>
		protected override string GetLocalizedString(string value)
		{
			if (this.localizedString == null)
			{
				var resourceManager = new ResourceManager(this.ResourceType);
				if (resourceManager != null)
				{
					try
					{
						this.localizedString = resourceManager.GetString(value, CultureInfo.CurrentUICulture);
					}
					catch (MissingManifestResourceException)
					{
						// Ignore invalid resources
					}
				}
			}

			return this.localizedString ?? base.GetLocalizedString(value);
		}
	}
}