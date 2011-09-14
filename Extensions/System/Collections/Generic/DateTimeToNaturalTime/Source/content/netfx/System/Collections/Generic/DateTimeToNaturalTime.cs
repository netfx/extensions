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

/// <summary>
/// Converts a date time to a readable natural time (i.e. '10 hours', '2 years') or relative time (i.e. '5 days ago').
/// </summary>
/// <nuget id="netfx-System.Collections.Generic.DateTimeToNaturalTime"/>
internal static partial class DateTimeToNaturalTimeExtension
{
	/// <summary>
	/// Renders the given date as a natural language string, appending the
	/// "ago" suffix like "10 minutes ago".
	/// </summary>
	/// <param name="date" this="true">The date to convert.</param>
	public static string ToNaturalRelativeTime(this DateTime date)
	{
		return ToNaturalString(date, true);
	}

	/// <summary>
	/// Renders the given date as a natural language string, like "10 minutes".
	/// </summary>
	/// <param name="date" this="true">The date to convert.</param>
	public static string ToNaturalTime(this DateTime date)
	{
		return ToNaturalString(date, false);
	}

	/// <summary>
	/// Renders the given date as a natural language string, appending the
	/// "ago" suffix like "10 minutes ago".
	/// </summary>
	/// <param name="date" this="true">The date to convert.</param>
	public static string ToNaturalRelativeTime(this DateTimeOffset date)
	{
		return ToNaturalString(date.UtcDateTime, true);
	}

	/// <summary>
	/// Renders the given date as a natural language string, like "10 minutes".
	/// </summary>
	/// <param name="date" this="true">The date to convert.</param>
	public static string ToNaturalTime(this DateTimeOffset date)
	{
		return ToNaturalString(date.UtcDateTime, false);
	}

	private static string ToNaturalString(this DateTime date, bool prependAgo)
	{
		if (date.Kind == DateTimeKind.Unspecified)
			throw new NotSupportedException("Date must be in UTC or local time. Unsupported 'Unspecified' date kind.");

		if (date.Kind != DateTimeKind.Utc)
			date = date.ToUniversalTime();

		var totalSeconds = ((TimeSpan)(DateTime.UtcNow - date)).TotalSeconds;

		var years = Math.Floor(totalSeconds / (365.242199 * 24 * 60 * 60));
		if (years >= 1)
		{
			return string.Format((years == 1 ?
				(prependAgo ? "{0} year ago" : "{0} year") :
				(prependAgo ? "{0} years ago" : "{0} years")),
				years);
		}
		var months = Math.Floor(totalSeconds / (30.4368499 * 24 * 60 * 60));
		if (months >= 1)
		{
			return string.Format((months == 1 ?
				(prependAgo ? "{0} month ago" : "{0} month") :
				(prependAgo ? "{0} months ago" : "{0} months")),
				months);
		}

		var weeks = Math.Floor(totalSeconds / (7 * 24 * 60 * 60));
		if (weeks >= 1)
		{
			return string.Format((weeks == 1 ?
				(prependAgo ? "{0} week ago" : "{0} week") :
				(prependAgo ? "{0} weeks ago" : "{0} weeks")),
				weeks);
		}

		var days = Math.Floor(totalSeconds / (24 * 60 * 60));
		if (days >= 1)
		{
			return string.Format((days == 1 ?
				(prependAgo ? "{0} day ago" : "{0} day") :
				(prependAgo ? "{0} days ago" : "{0} days")),
				days);
		}

		var hours = Math.Floor(totalSeconds / (60 * 60));
		if (hours >= 1)
		{
			return string.Format((hours == 1 ?
				(prependAgo ? "{0} hour ago" : "{0} hour") :
				(prependAgo ? "{0} hours ago" : "{0} hours")),
				hours);
		}

		var minutes = Math.Floor(totalSeconds / 60);
		if (minutes < 0) minutes = 0;

		return string.Format((minutes == 1 ?
			(prependAgo ? "{0} min ago" : "{0} min") :
			(prependAgo ? "{0} mins ago" : "{0} mins")),
			minutes);
	}
}