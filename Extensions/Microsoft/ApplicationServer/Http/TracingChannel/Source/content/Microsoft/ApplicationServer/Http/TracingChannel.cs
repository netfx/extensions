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
using System.Text;
using System.Net.Http;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Http.Headers;

namespace Microsoft.ApplicationServer.Http
{
	public class TracingChannel : DelegatingChannel
	{
		private static readonly ITraceSource tracer = Tracer.GetSourceFor<TracingChannel>();

		public TracingChannel(HttpMessageChannel handler)
			: base(handler)
		{
		}

		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			var body = "";
			if (request.Content != null)
				body = request.Content.ReadAsString();


			var builder = new StringBuilder();
			builder.AppendFormat("{0} {1} HTTP/{2}.{3}", request.Method, request.RequestUri, request.Version.Major, request.Version.Minor);
			builder.AppendLine();
			AddHeaders(builder, request.Headers);
			AddHeaders(builder, request.Content.Headers);

			tracer.TraceInformation(builder.ToString());
			if (!string.IsNullOrEmpty(body))
				tracer.TraceVerbose(body);

			return base.SendAsync(request, cancellationToken)
				.ContinueWith(task =>
				{
					if (task.Result.Content != null)
					{
						builder = new StringBuilder();
						builder.AppendFormat("HTTP/{0}.{1} {2} {3}", task.Result.Version.Major, task.Result.Version.Minor, (int)task.Result.StatusCode, task.Result.ReasonPhrase);
						builder.AppendLine();
						builder.AppendLine("Date: " + DateTime.Now.ToString("r"));

						AddHeaders(builder, task.Result.Headers);
						AddHeaders(builder, task.Result.Content.Headers);

						tracer.TraceInformation(builder.ToString());

						tracer.TraceVerbose(task.Result.Content.ReadAsString());
					}

					return task.Result;
				}, cancellationToken);
		}

		private void AddHeaders(StringBuilder builder, HttpHeaders headers)
		{
			foreach (var key in headers)
			{
				builder.AppendLine(key.Key + ": " + string.Join(", ", key.Value));
			}
		}
	}
}