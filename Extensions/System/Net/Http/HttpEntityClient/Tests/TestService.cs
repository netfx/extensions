using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Web;

[ServiceContract]
public class TestService
{
	private List<Product> products;

	public TestService()
	{
		this.products = new List<Product>
		{
			new Product
			{
				Id = 1,
				Owner = new User
				{
					Id = 1,
					Name = "kzu",
				}
			}, 
			new Product
			{
				Id = 2,
				Owner = new User
				{
					Id = 1,
					Name = "kzu",
				}
			}, 
			new Product
			{
				Id = 3,
				Owner = new User
				{
					Id = 2,
					Name = "vga",
				}
			}, 
		};
	}

	[WebGet(UriTemplate = "{id}")]
	public Product Get(int id)
	{
		return this.products.FirstOrDefault(x => x.Id == id);
	}

	[WebGet(UriTemplate = "")]
	public IQueryable<Product> All()
	{
		return this.products.AsQueryable();
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
