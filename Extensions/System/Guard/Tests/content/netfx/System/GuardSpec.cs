using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

public class GuardSpec
{
	[Fact]
	public void WhenNullVariableReferencePassed_ThenThrowsArgumentNullExceptionWithVariableName()
	{
		string value = null;

		var ex = Assert.Throws<ArgumentNullException>(() => Guard.NotNull(() => value, value));

        Assert.Equal("value", ex.ParamName);
    }

	[Fact]
	public void WhenNullParameterPassed_ThenThrowsArgumentNullExceptionWithParameterName()
	{
		var ex = Assert.Throws<ArgumentNullException>(() => Do(null));

        Assert.Equal("value", ex.ParamName);
	}

	[Fact]
	public void WhenNullStringVariablePassed_ThenThrowsArgumentNullExceptionWithVariableName()
	{
		string value = null;

		var ex  = Assert.Throws<ArgumentNullException>(() => Guard.NotNullOrEmpty(() => value, value));

        Assert.Equal("value", ex.ParamName);
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

		var ex = Assert.Throws<ArgumentException>(() => Guard.NotNullOrEmpty(() => value, value));

        Assert.Equal("value", ex.ParamName);
	}

    [Fact]
    public void WhenValueIsValid_ThenNoOp()
    {
        var value = "foo";

        Guard.IsValid(() => value, value, s => true, "Invalid");
    }

    [Fact]
    public void WhenValueIsInvalid_ThenThrows()
    {
        var value = "foo";

        var ex = Assert.Throws<ArgumentException>(() => Guard.IsValid(() => value, value, s => false, "Invalid"));

        Assert.Equal("value", ex.ParamName);
    }

    [Fact]
    public void WhenValueIsInvalid_ThenThrowsWithFormat()
    {
        var value = "foo";

        var ex = Assert.Throws<ArgumentException>(() => Guard.IsValid(() => value, value, s => false, "Invalid {0}", "bar"));

        Assert.Equal("value", ex.ParamName);
        Assert.True(ex.Message.StartsWith("Invalid bar"));
    }

	private void Do(string value)
	{
		Guard.NotNull(() => value, value);
	}
}
