using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.IO;

internal interface IEntityFormatter
{
	T FromContent<T>(HttpContent content);
	HttpContent ToContent<T>(T entity);
}
