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
using System.Net.Http.Headers;

/// <summary>
/// A client API that communicates with REST services implemented 
/// using standard REST methods and WCF Web API for querying.
/// </summary>
internal class HttpEntityClient
{
	private HttpClient http;
	private IEntityFormatter formatter;

	/// <summary>
	/// Initializes a new instance of the <see cref="HttpEntityClient"/> class.
	/// </summary>
	/// <param name="baseAddress">The base address of the service.</param>
	/// <param name="formatter">The formatter that translates service responses into entitites.</param>
	public HttpEntityClient(string baseAddress, IEntityFormatter formatter)
		: this(new Uri(baseAddress), formatter)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="HttpEntityClient"/> class.
	/// </summary>
	/// <param name="baseAddress">The base address of the service.</param>
	/// <param name="formatter">The formatter that translates service responses into entitites.</param>
	public HttpEntityClient(Uri baseAddress, IEntityFormatter formatter)
	{
		this.http = new HttpClient(baseAddress);
		this.http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(formatter.ContentType));
		this.formatter = formatter;
	}

	public T Get<T>(string requestUri)
	{
		return this.Get<T>(AsUri(requestUri));
	}

	public T Get<T>(Uri requestUri)
	{
		var entity = default(T);
		var response = TryGet<T>(requestUri, out entity);

		if (!response.IsSuccessStatusCode)
			throw new HttpResponseException(response);

		return entity;
	}

	public HttpResponseMessage TryGet<T>(string requestUri, out T entity)
	{
		return this.TryGet<T>(AsUri(requestUri), out entity);
	}

	public HttpResponseMessage TryGet<T>(Uri requestUri, out T entity)
	{
		var response = this.http.Get(requestUri);
		entity = default(T);

		if (response.IsSuccessStatusCode)
		{
			ThrowIfUnsupportedContentType(response);
			entity = this.formatter.FromContent<T>(response.Content);
		}

		return response;
	}

	private void ThrowIfUnsupportedContentType(HttpResponseMessage response)
	{
		if (response.Content.Headers.ContentType.MediaType != this.formatter.ContentType)
			throw new NotSupportedException(string.Format(
				"Received reponse with content type '{0}' but formatter supports '{1}'.",
				response.Content.Headers.ContentType,
				this.formatter.ContentType));
	}

	private Uri AsUri(string requestUri)
	{
		if (string.IsNullOrEmpty(requestUri))
			return default(Uri);

		return new Uri(requestUri, UriKind.RelativeOrAbsolute);
	}

}
