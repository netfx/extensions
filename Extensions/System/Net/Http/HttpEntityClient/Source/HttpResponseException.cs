using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Net;

internal class HttpResponseException : HttpException
{
	public HttpResponseException(HttpResponseMessage response)
		: base(response.ReasonPhrase)
	{
		this.Response = response;
	}

	public HttpStatusCode StatusCode { get { return this.Response.StatusCode; } }

	public HttpResponseMessage Response { get; private set; }
}

