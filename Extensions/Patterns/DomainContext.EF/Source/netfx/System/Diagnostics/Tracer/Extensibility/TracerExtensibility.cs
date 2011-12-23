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
using System.Linq;
using System.Diagnostics.Extensibility;

namespace System.Diagnostics
{
	/// <summary>
	/// Provides low-level manipulation of the current <see cref="ITracer"/> implementation.
	/// </summary>
	static partial class TracerExtensibility
	{
		/// <summary>
		/// Sets the <see cref="Tracer"/> internal <see cref="ITracer"/> implementation.
		/// </summary>
		/// <param name="tracer">The tracer to replace the default diagnostics tracer with.</param>
		/// <returns>An object that restores the original tracer when it's disposed (optional).</returns>
		public static IDisposable SetTracer(ITracer tracer)
		{
			Guard.NotNull(() => tracer, tracer);

			return new TracerReplacer(tracer);
		}

		/// <summary>
		/// Sets the tracing level for the source with the given <paramref name="sourceName"/>
		/// </summary>
		public static void SetTracingLevel(string sourceName, SourceLevels level)
		{
			Guard.NotNullOrEmpty(() => sourceName, sourceName);

			Tracer.Instance.GetSourceEntryFor(sourceName).Configuration.Switch.Level = level;
		}

		/// <summary>
		/// Adds a listener to the source with the given <paramref name="sourceName"/>.
		/// </summary>
		public static void AddListener(string sourceName, TraceListener listener)
		{
			Guard.NotNullOrEmpty(() => sourceName, sourceName);
			Guard.NotNull(() => listener, listener);

			Tracer.Instance.AddListener(sourceName, listener);
		}

		/// <summary>
		/// Removes a listener from the source with the given <paramref name="sourceName"/>.
		/// </summary>
		public static void RemoveListener(string sourceName, string listenerName)
		{
			Tracer.Instance.RemoveListener(sourceName, listenerName);
		}

		/// <summary>
		/// Removes the listener with the given name from the specified source.
		/// </summary>
		public static void RemoveListener(this ITracer tracer, string sourceName, string listenerName)
		{
			Guard.NotNull(() => tracer, tracer);
			Guard.NotNullOrEmpty(() => sourceName, sourceName);
			Guard.NotNullOrEmpty(() => listenerName, listenerName);

			var toRemove = tracer.GetSourceEntryFor(sourceName).Configuration.Listeners
				.Where(x => x.Name == listenerName).ToArray();

			foreach (var listener in toRemove)
			{
				tracer.RemoveListener(sourceName, listener);
			}
		}

		private class TracerReplacer : IDisposable
		{
			private ITracer oldTracer;
			private ITracer newTracer;

			public TracerReplacer(ITracer newTracer)
			{
				this.oldTracer = Tracer.Instance;
				this.newTracer = newTracer;

				Tracer.Instance = newTracer;
			}

			public void Dispose()
			{
				Tracer.Instance = this.oldTracer;
			}
		}
	}
}