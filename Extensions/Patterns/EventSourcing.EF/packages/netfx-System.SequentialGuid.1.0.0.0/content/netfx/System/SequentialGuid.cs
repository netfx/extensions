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
/// A safe managed sequential GUID generator (or Comb) that improves 
/// performance of GUID-style identifiers used in persistence.
/// </summary>
/// <devdoc>Source from NHibernate Guid Comb generator: http://nhibernate.svn.sourceforge.net/viewvc/nhibernate/trunk/nhibernate/src/NHibernate/Id/GuidCombGenerator.cs </devdoc>
///	<nuget id="netfx-System.SequentialGuid" />
internal static partial class SequentialGuid
{
	/// <summary>
	/// Creates a new sequential guid.
	/// </summary>
	public static Guid NewGuid()
	{
		byte[] guidArray = Guid.NewGuid().ToByteArray();

		DateTime baseDate = new DateTime(1900, 1, 1);
		DateTime now = DateTime.Now;

		// Get the days and milliseconds which will be used to build the byte string 
		TimeSpan days = new TimeSpan(now.Ticks - baseDate.Ticks);
		TimeSpan msecs = now.TimeOfDay;

		// Convert to a byte array 
		// Note that SQL Server is accurate to 1/300th of a millisecond so we divide by 3.333333 
		byte[] daysArray = BitConverter.GetBytes(days.Days);
		byte[] msecsArray = BitConverter.GetBytes((long)(msecs.TotalMilliseconds / 3.333333));

		// Reverse the bytes to match SQL Servers ordering 
		Array.Reverse(daysArray);
		Array.Reverse(msecsArray);

		// Copy the bytes into the guid 
		Array.Copy(daysArray, daysArray.Length - 2, guidArray, guidArray.Length - 6, 2);
		Array.Copy(msecsArray, msecsArray.Length - 4, guidArray, guidArray.Length - 4, 4);

		return new Guid(guidArray);
	}
}