using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Json format to use with <see cref="JsonNetEntityFormatter"/>.
/// </summary>
internal enum JsonNetFormat
{
	/// <summary>
	/// Json Text.
	/// </summary>
	Json,

	/// <summary>
	/// Binary Json.
	/// </summary>
	Bson,
}
