#region BSD License
/* 
Copyright (c) 2011, NETFx
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, 
are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this list 
  of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice, this 
  list of conditions and the following disclaimer in the documentation and/or other 
  materials provided with the distribution.

* Neither the name of Clarius Consulting nor the names of its contributors may be 
  used to endorse or promote products derived from this software without specific 
  prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY 
EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES 
OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT 
SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, 
INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED 
TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR 
BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN 
ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH 
DAMAGE.
*/
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

public class DateTimeToNaturalTimeSpec
{
	[Fact]
	public void WhenOneYearTime_ThenRendersNaturalTime()
	{
		var date = DateTime.Now.Subtract(TimeSpan.FromDays(366));
		var offset = DateTimeOffset.Now.Subtract(TimeSpan.FromDays(366));

		Assert.Equal("1 year", date.ToNaturalTime());
		Assert.Equal("1 year ago", date.ToNaturalRelativeTime());
		Assert.Equal("1 year", offset.ToNaturalTime());
		Assert.Equal("1 year ago", offset.ToNaturalRelativeTime());
	}

	[Fact]
	public void WhenTwoYearsTime_ThenRendersNaturalTime()
	{
		var date = DateTime.Now.Subtract(TimeSpan.FromDays(800));
		var offset = DateTimeOffset.Now.Subtract(TimeSpan.FromDays(800));

		Assert.Equal("2 years", date.ToNaturalTime());
		Assert.Equal("2 years ago", date.ToNaturalRelativeTime());
		Assert.Equal("2 years", offset.ToNaturalTime());
		Assert.Equal("2 years ago", offset.ToNaturalRelativeTime());
	}

	[Fact]
	public void WhenOneMonthTime_ThenRendersNaturalTime()
	{
		var date = DateTime.Now.Subtract(TimeSpan.FromDays(32));
		var offset = DateTimeOffset.Now.Subtract(TimeSpan.FromDays(32));

		Assert.Equal("1 month", date.ToNaturalTime());
		Assert.Equal("1 month ago", date.ToNaturalRelativeTime());
		Assert.Equal("1 month", offset.ToNaturalTime());
		Assert.Equal("1 month ago", offset.ToNaturalRelativeTime());
	}

	[Fact]
	public void WhenTwoMonthsTime_ThenRendersNaturalTime()
	{
		var date = DateTime.Now.Subtract(TimeSpan.FromDays(65));
		var offset = DateTimeOffset.Now.Subtract(TimeSpan.FromDays(65));

		Assert.Equal("2 months", date.ToNaturalTime());
		Assert.Equal("2 months ago", date.ToNaturalRelativeTime());
		Assert.Equal("2 months", offset.ToNaturalTime());
		Assert.Equal("2 months ago", offset.ToNaturalRelativeTime());
	}

	[Fact]
	public void WhenOneWeekTime_ThenRendersNaturalTime()
	{
		var date = DateTime.Now.Subtract(TimeSpan.FromDays(8));
		var offset = DateTimeOffset.Now.Subtract(TimeSpan.FromDays(8));

		Assert.Equal("1 week", date.ToNaturalTime());
		Assert.Equal("1 week ago", date.ToNaturalRelativeTime());
		Assert.Equal("1 week", offset.ToNaturalTime());
		Assert.Equal("1 week ago", offset.ToNaturalRelativeTime());
	}

	[Fact]
	public void WhenTwoWeeksTime_ThenRendersNaturalTime()
	{
		var date = DateTime.Now.Subtract(TimeSpan.FromDays(15));
		var offset = DateTimeOffset.Now.Subtract(TimeSpan.FromDays(15));

		Assert.Equal("2 weeks", date.ToNaturalTime());
		Assert.Equal("2 weeks ago", date.ToNaturalRelativeTime());
		Assert.Equal("2 weeks", offset.ToNaturalTime());
		Assert.Equal("2 weeks ago", offset.ToNaturalRelativeTime());
	}

	[Fact]
	public void WhenOneDayTime_ThenRendersNaturalTime()
	{
		var date = DateTime.Now.Subtract(TimeSpan.FromDays(1));
		var offset = DateTimeOffset.Now.Subtract(TimeSpan.FromDays(1));

		Assert.Equal("1 day", date.ToNaturalTime());
		Assert.Equal("1 day ago", date.ToNaturalRelativeTime());
		Assert.Equal("1 day", offset.ToNaturalTime());
		Assert.Equal("1 day ago", offset.ToNaturalRelativeTime());
	}

	[Fact]
	public void WhenTwoDaysTime_ThenRendersNaturalTime()
	{
		var date = DateTime.Now.Subtract(TimeSpan.FromDays(2));
		var offset = DateTimeOffset.Now.Subtract(TimeSpan.FromDays(2));

		Assert.Equal("2 days", date.ToNaturalTime());
		Assert.Equal("2 days ago", date.ToNaturalRelativeTime());
		Assert.Equal("2 days", offset.ToNaturalTime());
		Assert.Equal("2 days ago", offset.ToNaturalRelativeTime());
	}

	[Fact]
	public void WhenOneHourTime_ThenRendersNaturalTime()
	{
		var date = DateTime.Now.Subtract(TimeSpan.FromHours(1));
		var offset = DateTimeOffset.Now.Subtract(TimeSpan.FromHours(1));

		Assert.Equal("1 hour", date.ToNaturalTime());
		Assert.Equal("1 hour ago", date.ToNaturalRelativeTime());
		Assert.Equal("1 hour", offset.ToNaturalTime());
		Assert.Equal("1 hour ago", offset.ToNaturalRelativeTime());
	}

	[Fact]
	public void WhenTwoHoursTime_ThenRendersNaturalTime()
	{
		var date = DateTime.Now.Subtract(TimeSpan.FromHours(2));
		var offset = DateTimeOffset.Now.Subtract(TimeSpan.FromHours(2));

		Assert.Equal("2 hours", date.ToNaturalTime());
		Assert.Equal("2 hours ago", date.ToNaturalRelativeTime());
		Assert.Equal("2 hours", offset.ToNaturalTime());
		Assert.Equal("2 hours ago", offset.ToNaturalRelativeTime());
	}

	[Fact]
	public void WhenNoMinutesTime_ThenRendersNaturalTime()
	{
		var date = DateTime.Now.Subtract(TimeSpan.FromSeconds(10));
		var offset = DateTime.Now.Subtract(TimeSpan.FromSeconds(10));

		Assert.Equal("0 mins", date.ToNaturalTime());
		Assert.Equal("0 mins ago", date.ToNaturalRelativeTime());
		Assert.Equal("0 mins", offset.ToNaturalTime());
		Assert.Equal("0 mins ago", offset.ToNaturalRelativeTime());
	}

	[Fact]
	public void WhenOneMinuteTime_ThenRendersNaturalTime()
	{
		var date = DateTime.Now.Subtract(TimeSpan.FromMinutes(1));
		var offset = DateTimeOffset.Now.Subtract(TimeSpan.FromMinutes(1));

		Assert.Equal("1 min", date.ToNaturalTime());
		Assert.Equal("1 min ago", date.ToNaturalRelativeTime());
		Assert.Equal("1 min", offset.ToNaturalTime());
		Assert.Equal("1 min ago", offset.ToNaturalRelativeTime());
	}

	[Fact]
	public void WhenTwoMinutesTime_ThenRendersNaturalTime()
	{
		var date = DateTime.Now.Subtract(TimeSpan.FromMinutes(2));
		var offset = DateTimeOffset.Now.Subtract(TimeSpan.FromMinutes(2));

		Assert.Equal("2 mins", date.ToNaturalTime());
		Assert.Equal("2 mins ago", date.ToNaturalRelativeTime());
		Assert.Equal("2 mins", offset.ToNaturalTime());
		Assert.Equal("2 mins ago", offset.ToNaturalRelativeTime());
	}

	[Fact]
	public void WhenDateKindUnspecified_ThenThrowsNotSupported()
	{
		var date = new DateTime(2011, 1, 1, 1, 1, 1, DateTimeKind.Unspecified);

		Assert.Throws<NotSupportedException>(() => date.ToNaturalTime());
	}
}