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
	/// Represents a trace event that contains a message.
	/// </summary>
	partial class MessageTraceEvent : TraceEvent
	{
		private Lazy<string> messageOrFormat;

		/// <summary>
		/// Initializes a new instance of the <see cref="MessageTraceEvent"/> class.
		/// </summary>
		public MessageTraceEvent(TraceEventType eventType, int eventId, string message)
			: this(eventType, eventId, new Lazy<string>(() => message))
		{
			Guard.NotNullOrEmpty(() => message, message);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MessageTraceEvent"/> class.
		/// </summary>
		public MessageTraceEvent(TraceEventType eventType, int eventId, string format, params object[] args)
			: this(eventType, eventId, new Lazy<string>(() => format))
		{
			Guard.NotNullOrEmpty(() => format, format);
			Guard.NotNull(() => args, args);

			if (args.Length == 0)
				this.MessageFormatArgs = null;
			else
				this.MessageFormatArgs = args;
		}

		/// <summary>
		/// Used by derived classes to provide a lazy-calculated value for the <see cref="MessageOrFormat"/> property.
		/// </summary>
		protected MessageTraceEvent(TraceEventType eventType, int eventId, Lazy<string> message)
			: base(eventType, eventId)
		{
			Guard.NotNull(() => message, message);

			this.messageOrFormat = message;
		}

		/// <summary>
		/// Gets the message or format string to use in combination with <see cref="MessageFormatArgs"/>.
		/// </summary>
		public string MessageOrFormat { get { return this.messageOrFormat.Value; } }

		/// <summary>
		/// Gets the message format string args to perform token replacement.
		/// </summary>
		/// <value>The message format args.</value>
		public object[] MessageFormatArgs { get; private set; }

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents this instance.
		/// </summary>
		public override string ToString()
		{
			return this.Type + ": " +
				(this.MessageFormatArgs != null ?
					string.Format(this.MessageOrFormat, this.MessageFormatArgs) :
					this.MessageOrFormat);
		}
	}
}
