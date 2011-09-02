#region BSD License
/* 
Copyright (c) 2010, NETFx
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

* Neither the name of Clarius Consulting nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Reflection;

/// <summary>
/// Represents the filter criteria for a message store query.
/// </summary>
/// <nuget id="netfx-Patterns.MessageStore"/>
partial class MessageStoreQueryCriteria
{
	/// <summary>
	/// Initializes a new instance of the <see cref="MessageStoreQueryCriteria"/> class.
	/// </summary>
	public MessageStoreQueryCriteria()
	{
		this.MessageTypes = new HashSet<Type>();
	}

	/// <summary>
	/// List of message type filters. All types added are OR'ed with the 
	/// others (i.e. <c>MessageType == InvalidServerAddress OR MessageType == ServerMemoryLow</c>).
	/// </summary>
	public HashSet<Type> MessageTypes { get; private set; }

	/// <summary>
	/// Filters messages that happened after the given starting date.
	/// </summary>
	public DateTime? Since { get; set; }

	/// <summary>
	/// Filters messages that happened before the given ending date.
	/// </summary>
	public DateTime? Until { get; set; }

	/// <summary>
	/// If set to <see langword="true"/>, <see cref="Since"/> and <see cref="Until"/> should 
	/// be considered as exclusive ranges (excludes values with a matching date). 
	/// Defaults to <see langword="false"/>, meaning that ranges are inclusive by default.
	/// </summary>
	public bool IsExclusiveRange { get; set; }
}
