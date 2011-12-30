using System;
using Xunit;

public class GuardSpec
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
	public void WhenNonEmptyStringVariablePassed_ThenNoOp()
	{
		var value = "foo";

		Guard.NotNullOrEmpty(() => value, value);
	}

	[Fact]
	public void WhenNonNullStringVariablePassed_ThenNoOp()
	{
		var value = "foo";

		Guard.NotNull(() => value, value);
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
