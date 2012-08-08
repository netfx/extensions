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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Events;
using System.Linq;
using System.Reflection;

namespace System.Diagnostics.Extensibility
{
    /// <summary>
    /// Implements the <see cref="ITracer"/> interface on top of 
    /// <see cref="TraceSource"/>.
    /// </summary>
    internal class DiagnosticsTracer : ITracer
    {
        public ITraceSourceEntry GetSourceEntryFor(string name)
        {
            return new DiagnosticsTraceSourceEntry(this.GetOrAdd(name, s => new TraceSource(s)));
        }

        public void AddListener(string sourceName, TraceListener listener)
        {
            this.GetOrAdd(sourceName, name => new TraceSource(name)).Listeners.Add(listener);
        }

        public void RemoveListener(string sourceName, TraceListener listener)
        {
            this.GetOrAdd(sourceName, name => new TraceSource(name)).Listeners.Remove(listener);
        }

        /// <summary>
        /// Gets an AppDomain-cached trace source of the given name, or creates it.
        /// </summary>
        private TraceSource GetOrAdd(string sourceName, Func<string, TraceSource> factory)
        {
            var cachedSources = AppDomain.CurrentDomain.GetData<Dictionary<string, TraceSource>>();
            if (cachedSources == null)
            {
                // This lock guarantees that throughout the current 
                // app domain, only a single root trace source is 
                // created ever.
                lock (AppDomain.CurrentDomain)
                {
                    cachedSources = AppDomain.CurrentDomain.GetData<Dictionary<string, TraceSource>>();
                    if (cachedSources == null)
                    {
                        cachedSources = new Dictionary<string, TraceSource>();
                        AppDomain.CurrentDomain.SetData(cachedSources);
                    }
                }
            }

            return cachedSources.GetOrAdd(sourceName, factory);
        }

        /// <summary>
        /// Provides access to the trace source as well as its 
        /// underlying switch and listeners.
        /// </summary>
        private class DiagnosticsTraceSourceEntry : ITraceSourceEntry
        {
            private DiagnosticsTraceSourceAdapter adapter;

            public DiagnosticsTraceSourceEntry(TraceSource source)
            {
                this.adapter = new DiagnosticsTraceSourceAdapter(source);
            }

            public ITraceSourceConfiguration Configuration { get { return this.adapter; } }

            public ITraceSource TraceSource { get { return this.adapter; } }
        }

        private class DiagnosticsTraceSourceAdapter : ITraceSource, ITraceSourceConfiguration, IDiagnosticsTraceSource
        {
            // Private reflection needed here in order to make the inherited source names still 
            // log as if the original source name was the one logging, so as not to lose the 
            // originating class name.
            private static readonly FieldInfo sourceNameField = typeof(TraceSource).GetField("sourceName", BindingFlags.Instance | BindingFlags.NonPublic);
            private TraceSource traceSource;

            public DiagnosticsTraceSourceAdapter(TraceSource source)
            {
                this.traceSource = source;
            }

            public string Name
            {
                get { return this.traceSource.Name; }
            }

            public void Flush()
            {
                this.traceSource.Flush();
            }

            public SourceSwitch Switch
            {
                get { return this.traceSource.Switch; }
                set { this.traceSource.Switch = value; }
            }

            public ICollection<TraceListener> Listeners
            {
                get { return new ListenersCollection(this.traceSource); }
            }

            public void Trace(TraceEvent traceEvent)
            {
                var transferEvent = traceEvent as TransferTraceEvent;
                var dataEvent = traceEvent as DataTraceEvent;
                var messageEvent = traceEvent as MessageTraceEvent;

                if (transferEvent != null)
                    this.traceSource.TraceTransfer(transferEvent.Id, transferEvent.MessageOrFormat, transferEvent.RelatedActivityId);
                else if (dataEvent != null)
                    this.traceSource.TraceData(dataEvent.Type, dataEvent.Id, dataEvent.Data);
                else if (messageEvent != null)
                    this.traceSource.TraceEvent(messageEvent.Type, messageEvent.Id, messageEvent.MessageOrFormat, messageEvent.MessageFormatArgs);
            }

            public void Trace(string originalSourceName, TraceEvent traceEvent)
            {
                var currentName = this.traceSource.Name;
                // Transient change of the source name while the trace call 
                // is issued.
                sourceNameField.SetValue(this.traceSource, originalSourceName);
                try
                {
                    Trace(traceEvent);
                }
                finally
                {
                    sourceNameField.SetValue(this.traceSource, currentName);
                }
            }

            private class ListenersCollection : Collection<TraceListener>
            {
                private TraceSource traceSource;

                public ListenersCollection(TraceSource traceSource)
                    : base(traceSource.Listeners.OfType<TraceListener>().ToList())
                {
                    this.traceSource = traceSource;
                }

                protected override void InsertItem(int index, TraceListener item)
                {
                    base.InsertItem(index, item);

                    this.traceSource.Listeners.Add(item);
                }

                protected override void RemoveItem(int index)
                {
                    base.RemoveItem(index);

                    this.traceSource.Listeners.RemoveAt(index);
                }

                protected override void ClearItems()
                {
                    base.ClearItems();

                    this.traceSource.Listeners.Clear();
                }
            }
        }
    }
}
