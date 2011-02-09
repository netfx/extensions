using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
}
