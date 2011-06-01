using System;
using Xunit;

internal class DateTimeEpochExtensionsSpec
{
	[Fact]
	public void WhenConvertingToEpoch_ThenDateTimeEqualsOffset()
	{
		var offset = DateTimeOffset.Now;
		var date = offset.ToLocalTime().DateTime;

		Assert.Equal(offset.ToEpochTime(), date.ToEpochTime());
	}

	[Fact]
	public void WhenRoundtripping_ThenSucceeds()
	{
		var offset = new DateTimeOffset(2011, 2, 7, 4, 30, 20, TimeSpan.Zero);

		Assert.Equal(0, offset.Millisecond);

		var roundtrip = offset.ToEpochTime().ToDateTimeOffsetFromEpoch();

		Assert.Equal(offset, roundtrip);
	}

	[Fact]
	public void WhenConvertingEpochTime_ThenReturnsZero()
	{
		var date = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		Assert.Equal(0, date.ToEpochTime());
	}

	[Fact]
	public void WhenConvertingEpochTimePlus10Seconds_ThenReturns10()
	{
		var date = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(10);

		Assert.Equal(10, date.ToEpochTime());
	}

	[Fact]
	public void WhenConvertingEpochTimeMinus10Seconds_ThenReturnsMinus10()
	{
		var date = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(-10);

		Assert.Equal(-10, date.ToEpochTime());
	}
}
