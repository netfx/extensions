using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.Net.Http.Entity;
using System.Data.Entity.Design.PluralizationServices;

namespace Tests
{
	public class PluralizerSpec
	{
		[Fact]
		public void WhenFormatPascal_ThenTurnsFirstLetterUpperCase()
		{
			var pluralizer = new PluralizerResourceConvention(
				PluralizationService.CreateService(new System.Globalization.CultureInfo("en-US")), PluralizerResourceFormat.PascalCase);

			Assert.Equal("HelloWorlds", pluralizer.GetResourceName(typeof(helloWorld)));
		}

		[Fact]
		public void WhenFormatLowerCase_ThenTurnsFirstLetterLowerCase()
		{
			var pluralizer = new PluralizerResourceConvention(
				PluralizationService.CreateService(new System.Globalization.CultureInfo("en-US")), PluralizerResourceFormat.CamelCase);

			Assert.Equal("helloWorlds", pluralizer.GetResourceName(typeof(HelloWorld)));
		}

		public class helloWorld { }
		public class HelloWorld { }
	}
}
