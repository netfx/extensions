using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Web;

[ServiceContract]
public class TestService
{
	[WebGet(UriTemplate = "{id}")]
	public Product Get(int id)
	{
		return new Product
		{
			Id = id,
			Owner = new User
			{
				Id = 1,
				Name = "kzu",
			}
		};
	}
}

public class Product
{
	public int Id { get; set; }
	public User Owner { get; set; }
}

public class User
{
	public int Id { get; set; }
	public string Name { get; set; }
}
