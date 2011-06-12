using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ApplicationServer.Http.Description;

/// <summary>
/// Provides the <see cref="UseJsonNet"/> extension method to turn 
/// on usage of Json.NET instead of the built-in Json serializer.
/// </summary>
internal static class JsonNetExtensions
{
	/// <summary>
	/// Makes Json.NET media type formatter the default for Json content types.
	/// </summary>
	public static IHttpHostConfigurationBuilder UseJsonNet(this IHttpHostConfigurationBuilder config)
	{
		config.Configuration.OperationHandlerFactory.Formatters.Insert(0, new JsonNetMediaTypeFormatter());

		return config;
	}
}
