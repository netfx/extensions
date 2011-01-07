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
using System.Xml.Serialization;
using System.IO;
using System.Xml;

/// <summary>
/// Serializes and deserializes objects into and from XML documents. 
/// </summary>
/// <typeparam name="TRoot">The type of the root object in the graph to serialize or deserialize.</typeparam>
internal class XmlSerializer<TRoot> : XmlSerializer
{
	/// <summary>
	/// Initializes a new instance of the <see cref="XmlSerializer&lt;TRoot&gt;"/> class.
	/// </summary>
	public XmlSerializer()
		: base(typeof(TRoot)) { }

	/// <summary>
	/// See <see cref="XmlSerializer(Type, Type[])"/>.
	/// </summary>
	public XmlSerializer(Type[] extraTypes)
		: base(typeof(TRoot), extraTypes) { }

	/// <summary>
	/// See <see cref="XmlSerializer(Type, Type[])"/>.
	/// </summary>
	public XmlSerializer(string defaultNamespace)
		: base(typeof(TRoot), defaultNamespace) { }

	/// <summary>
	/// See <see cref="XmlSerializer(Type, XmlAttributeOverrides)"/>.
	/// </summary>
	public XmlSerializer(XmlAttributeOverrides overrides)
		: base(typeof(TRoot), overrides) { }

	/// <summary>
	/// See <see cref="XmlSerializer(Type, XmlRootAttribute)"/>.
	/// </summary>
	public XmlSerializer(XmlRootAttribute root)
		: base(typeof(TRoot), root) { }

	/// <summary>
	/// See <see cref="XmlSerializer(Type, XmlAttributeOverrides, Type[], XmlRootAttribute, string)"/>.
	/// </summary>
	public XmlSerializer(XmlAttributeOverrides overrides, Type[] extraTypes, XmlRootAttribute root, string defaultNamespace)
		: base(typeof(TRoot), overrides, extraTypes, root, defaultNamespace) { }

	/// <summary>
	/// See <see cref="XmlSerializer(Type, XmlAttributeOverrides, Type[], XmlRootAttribute, string, string)"/>.
	/// </summary>
	public XmlSerializer(XmlAttributeOverrides overrides, Type[] extraTypes, XmlRootAttribute root, string defaultNamespace, string location)
		: base(typeof(TRoot), overrides, extraTypes, root, defaultNamespace, location) { }

	/// <summary>
	/// Deserializes the XML document contained in the specified file.
	/// </summary>
	public TRoot Deserialize(string fileName)
	{
		using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
		{
			return Deserialize(stream);
		}
	}

	/// <summary>
	/// See <see cref="XmlSerializer.Deserialize(Stream)"/>.
	/// </summary>
	public new TRoot Deserialize(Stream stream)
	{
		return (TRoot)base.Deserialize(stream);
	}

	/// <summary>
	/// See <see cref="XmlSerializer.Deserialize(Stream)"/>.
	/// </summary>
	public new TRoot Deserialize(TextReader textReader)
	{
		return (TRoot)base.Deserialize(textReader);
	}

	/// <summary>
	/// See <see cref="XmlSerializer.Deserialize(XmlReader)"/>.
	/// </summary>
	public new TRoot Deserialize(XmlReader xmlReader)
	{
		return (TRoot)base.Deserialize(xmlReader);
	}

	/// <summary>
	/// See <see cref="XmlSerializer.Deserialize(XmlReader, string)"/>.
	/// </summary>
	public new TRoot Deserialize(XmlReader xmlReader, string encodingStyle)
	{
		return (TRoot)base.Deserialize(xmlReader, encodingStyle);
	}

	/// <summary>
	/// See <see cref="XmlSerializer.Deserialize(XmlReader, XmlDeserializationEvents)"/>.
	/// </summary>
	public new TRoot Deserialize(XmlReader xmlReader, XmlDeserializationEvents events)
	{
		return (TRoot)base.Deserialize(xmlReader, events);
	}

	/// <summary>
	/// See <see cref="XmlSerializer.Deserialize(XmlReader, string, XmlDeserializationEvents)"/>.
	/// </summary>
	public new TRoot Deserialize(XmlReader xmlReader, string encodingStyle, XmlDeserializationEvents events)
	{
		return (TRoot)base.Deserialize(xmlReader, encodingStyle, events);
	}
}
