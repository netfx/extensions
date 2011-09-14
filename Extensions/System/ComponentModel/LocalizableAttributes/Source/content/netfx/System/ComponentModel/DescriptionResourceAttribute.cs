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
	public sealed class DescriptionResourceAttribute : DescriptionAttribute
	{
		private string localizedString;

		/// <summary>
		/// Initializes a new instance of the <see cref="DescriptionResourceAttribute"/> class.
		/// </summary>
		/// <param name="resourceName">The resource string.</param>
		/// <param name="resourceType">Type of the resource.</param>
		public DescriptionResourceAttribute(string resourceName, Type resourceType)
			: base(resourceName)
		{
			Guard.NotNull(() => resourceName, resourceName);
			Guard.NotNull(() => resourceType, resourceType);

			this.ResourceName = resourceName;
			this.ResourceType = resourceType;
		}

		/// <summary>
		/// Gets the description stored in this attribute.
		/// </summary>
		public override string Description
		{
			get
			{
				return this.GetLocalizedString();
			}
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
		private string GetLocalizedString()
		{
			if (this.localizedString == null)
			{
				var resourceManager = new ResourceManager(this.ResourceType);
				if (resourceManager != null)
				{
					try
					{
						this.localizedString = resourceManager.GetString(this.ResourceName, CultureInfo.CurrentUICulture);
					}
					catch (MissingManifestResourceException)
					{
						// Ignore invalid resources
					}
				}
			}

			return this.localizedString ?? this.ResourceName;
		}
	}
}