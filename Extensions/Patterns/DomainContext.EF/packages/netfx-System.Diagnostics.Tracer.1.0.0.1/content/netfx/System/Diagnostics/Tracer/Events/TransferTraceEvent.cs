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
namespace System.Diagnostics.Events
{
	/// <summary>
	/// Event traced whenever a new activity is started.
	/// </summary>
	[DebuggerDisplay("{Type} to {RelatedActivityId}: {MessageOrFormat}")]
	partial class TransferTraceEvent : MessageTraceEvent
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TransferTraceEvent"/> class.
		/// </summary>
		public TransferTraceEvent(Guid relatedActivityId, string message)
			: base(TraceEventType.Transfer, 0, message)
		{
			this.RelatedActivityId = relatedActivityId;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TransferTraceEvent"/> class.
		/// </summary>
		public TransferTraceEvent(Guid relatedActivityId, string format, params object[] args)
			: base(TraceEventType.Transfer, 0, new Lazy<string>(() => string.Format(format, args)))
		{
			Guard.NotNullOrEmpty(() => format, format);
			Guard.NotNull(() => args, args);

			this.RelatedActivityId = relatedActivityId;
		}

		/// <summary>
		/// Gets the activity that the event is transfering to.
		/// </summary>
		public Guid RelatedActivityId { get; private set; }
	}
}
