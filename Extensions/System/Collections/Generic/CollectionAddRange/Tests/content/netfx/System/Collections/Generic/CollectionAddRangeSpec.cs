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

public class CollectionAddRangeSpec
{
	[Fact]
	public void WhenAddingToNullCollection_ThenThrows()
	{
		var collection = (List<int>)null;

		Assert.Throws<ArgumentNullException>(() => CollectionAddRangeExtension.AddRange(collection, 3));
	}

	[Fact]
	public void WhenAddingNullItems_ThenThrows()
	{
		ICollection<int> ints = new List<int>(new[] { 1, 2, 3 });

		Assert.Throws<ArgumentNullException>(() => ints.AddRange((IEnumerable<int>)null));
	}

	[Fact]
	public void WhenAddingEnumerable_ThenAddsToList()
	{
		ICollection<int> ints = new List<int>(new[] { 1, 2, 3 });

		ints.AddRange(new[] { 4, 5 });

		Assert.Equal(4, ints.ToList()[3]);
		Assert.Equal(5, ints.ToList()[4]);
	}

	[Fact]
	public void WhenAddingParams_ThenAddsToList()
	{
		ICollection<int> ints = new List<int>(new[] { 1, 2, 3 });

		ints.AddRange(4, 5);

		Assert.Equal(4, ints.ToList()[3]);
		Assert.Equal(5, ints.ToList()[4]);
	}
}