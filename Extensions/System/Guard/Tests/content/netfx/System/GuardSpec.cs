using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

internal class GuardSpec
{
	[Fact]
	public void WhenNullVariableReferencePassed_ThenThrowsArgumentNullExceptionWithVariableName()
	{
		string value = null;

		Assert.Throws<ArgumentNullException>(() => Guard.NotNull(() => value, value));
	}

	[Fact]
	public void WhenNullParameterPassed_ThenThrowsArgumentNullExceptionWithParameterName()
	{
		Assert.Throws<ArgumentNullException>(() => Do(null));
	}

	[Fact]
	public void WhenNullStringVariablePassed_ThenThrowsArgumentNullExceptionWithVariableName()
	{
		string value = null;

		Assert.Throws<ArgumentNullException>(() => Guard.NotNullOrEmpty(() => value, value));
	}

	[Fact]
	public void WhenEmptyStringVariablePassed_ThenThrowsArgumentExceptionWithVariableName()
	{
		string value = String.Empty;

		Assert.Throws<ArgumentException>(() => Guard.NotNullOrEmpty(() => value, value));
	}

	private void Do(string value)
	{
		Guard.NotNull(() => value, value);
	}
}
