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

using System.Globalization;
using System.Diagnostics.Events;

namespace System.Diagnostics
{
	/// <summary>
	/// Provides usability overloads for tracing to a <see cref="ITraceSource"/>.
	/// </summary>
	static partial class TracerSourceExtensions
	{
		/// <summary>
		/// Starts a new activity scope.
		/// </summary>
		public static IDisposable StartActivity(this ITraceSource source, string format, params object[] args)
		{
			Guard.NotNull(() => source, source);
			Guard.NotNullOrEmpty(() => format, format);

			return new TraceActivity(source, format, args);
		}

		/// <summary>
		/// Starts a new activity scope.
		/// </summary>
		public static IDisposable StartActivity(this ITraceSource source, string displayName)
		{
			Guard.NotNull(() => source, source);
			Guard.NotNullOrEmpty(() => displayName, displayName);

			return new TraceActivity(source, displayName);
		}

		/// <summary>
		/// Traces data with the given associated <see cref="TraceEventType"/>.
		/// </summary>
		public static void TraceData(this ITraceSource source, TraceEventType eventType, object data)
		{
			Guard.NotNull(() => source, source);

			source.Trace(new DataTraceEvent(eventType, 0, data));
		}

		/// <summary>
		/// Traces an event of type <see cref="TraceEventType.Error"/> with the given format string and arguments.
		/// </summary>
		public static void TraceError(this ITraceSource source, string format, params object[] args)
		{
			Guard.NotNull(() => source, source);

			source.Trace(new MessageTraceEvent(TraceEventType.Error, 0, format, args));
		}

		/// <summary>
		/// Traces an event of type <see cref="TraceEventType.Error"/> with the given message;
		/// </summary>
		public static void TraceError(this ITraceSource source, string message)
		{
			Guard.NotNull(() => source, source);

			source.Trace(new MessageTraceEvent(TraceEventType.Error, 0, message));
		}

		/// <summary>
		/// Traces an event of type <see cref="TraceEventType.Error"/> with the given exception, format string and arguments.
		/// </summary>
		public static void TraceError(this ITraceSource source, Exception exception, string format, params object[] args)
		{
			Guard.NotNull(() => source, source);

			source.Trace(new ExceptionTraceEvent(TraceEventType.Error, 0, exception, format, args));
		}

		/// <summary>
		/// Traces an event of type <see cref="TraceEventType.Error"/> with the given exception and message.
		/// </summary>
		public static void TraceError(this ITraceSource source, Exception exception, string message)
		{
			Guard.NotNull(() => source, source);

			source.Trace(new ExceptionTraceEvent(TraceEventType.Error, 0, exception, message));
		}

		/// <summary>
		/// Traces an event of type <see cref="TraceEventType.Error"/> with the given exception, using the 
		/// <see cref="Exception.Message"/> as the trace event message.
		/// </summary>
		public static void TraceError(this ITraceSource source, Exception exception)
		{
			Guard.NotNull(() => source, source);

			source.Trace(new ExceptionTraceEvent(TraceEventType.Error, 0, exception));
		}

		/// <summary>
		/// Traces an event of type <see cref="TraceEventType.Information"/> with the given message;
		/// </summary>
		public static void TraceInformation(this ITraceSource source, string message)
		{
			Guard.NotNull(() => source, source);

			source.Trace(new MessageTraceEvent(TraceEventType.Information, 0, message));
		}

		/// <summary>
		/// Traces an event of type <see cref="TraceEventType.Information"/> with the given format string and arguments.
		/// </summary>
		public static void TraceInformation(this ITraceSource source, string format, params object[] args)
		{
			Guard.NotNull(() => source, source);

			source.Trace(new MessageTraceEvent(TraceEventType.Information, 0, format, args));
		}

		/// <summary>
		/// Traces an event of type <see cref="TraceEventType.Warning"/> with the given message;
		/// </summary>
		public static void TraceWarning(this ITraceSource source, string message)
		{
			Guard.NotNull(() => source, source);

			source.Trace(new MessageTraceEvent(TraceEventType.Warning, 0, message));
		}

		/// <summary>
		/// Traces an event of type <see cref="TraceEventType.Warning"/> with the given format string and arguments.
		/// </summary>
		public static void TraceWarning(this ITraceSource source, string format, params object[] args)
		{
			Guard.NotNull(() => source, source);

			source.Trace(new MessageTraceEvent(TraceEventType.Warning, 0, format, args));
		}

		/// <summary>
		/// Traces an event of type <see cref="TraceEventType.Verbose"/> with the given message.
		/// </summary>
		public static void TraceVerbose(this ITraceSource source, string message)
		{
			Guard.NotNull(() => source, source);

			source.Trace(new MessageTraceEvent(TraceEventType.Verbose, 0, message));
		}

		/// <summary>
		/// Traces an event of type <see cref="TraceEventType.Verbose"/> with the given format string and arguments.
		/// </summary>
		public static void TraceVerbose(this ITraceSource source, string format, params object[] args)
		{
			Guard.NotNull(() => source, source);

			source.Trace(new MessageTraceEvent(TraceEventType.Verbose, 0, format, args));
		}

		/// <devdoc>
		/// In order for activity tracing to happen, the trace source needs to 
		/// have <see cref="SourceLevels.ActivityTracing"/> enabled.
		/// </devdoc>
		[DebuggerStepThrough]
		private class TraceActivity : IDisposable
		{
			private string displayName;
			private object[] args;
			private bool disposed;
			private ITraceSource source;
			private Guid oldId;
			private Guid newId;

			public TraceActivity(ITraceSource source, string displayName)
				: this(source, displayName, null)
			{
			}

			public TraceActivity(ITraceSource source, string format, params object[] args)
			{
				Guard.NotNullOrEmpty(() => format, format);
				Guard.NotNull(() => source, source);

				this.displayName = format;
				if (args != null && args.Length > 0)
					this.args = args;

				this.source = source;
				this.newId = Guid.NewGuid();
				this.oldId = Trace.CorrelationManager.ActivityId;

				if (this.oldId != Guid.Empty)
				{
					if (this.args == null)
						source.Trace(new TransferTraceEvent(this.newId, this.oldId + " > " + this.newId, displayName));
					else
						source.Trace(new TransferTraceEvent(this.newId, this.oldId + " > " + this.newId, displayName, args));
				}

				Trace.CorrelationManager.ActivityId = newId;
				if (this.args == null)
					this.source.Trace(new MessageTraceEvent(TraceEventType.Start, 0, this.displayName));
				else
					this.source.Trace(new MessageTraceEvent(TraceEventType.Start, 0, this.displayName, args));
			}

			public void Dispose()
			{
				if (!this.disposed)
				{
					if (this.args == null)
						this.source.Trace(new MessageTraceEvent(TraceEventType.Stop, 0, this.displayName));
					else
						this.source.Trace(new MessageTraceEvent(TraceEventType.Stop, 0, this.displayName, args));

					if (this.oldId != Guid.Empty)
					{
						if (this.args == null)
							source.Trace(new TransferTraceEvent(this.oldId, this.newId + " > " + this.oldId, displayName));
						else
							source.Trace(new TransferTraceEvent(this.oldId, this.newId + " > " + this.oldId, displayName, args));
					}

					Trace.CorrelationManager.ActivityId = this.oldId;
				}

				this.disposed = true;
			}
		}
	}
}
