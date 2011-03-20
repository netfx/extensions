using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.CodeDom.Compiler;

internal class TraverseSpec
{
	/*
	 *	A
	 *		B1
	 *			C1
	 *			C2
	 *			C3
	 *		B2
	 *			C4
	 *			C5
	 *		B3
	 *			C6
	 * 
	*/
	private Node root = new Node
	{
		Name = "A",
		Nodes = 
		{
			new Node 
			{
				Name = "B1",
				Nodes = 
				{
					new Node { Name = "C1" },
					new Node { Name = "C2" },
					new Node { Name = "C3" },
				}
			}, 
			new Node 
			{
				Name = "B2",
				Nodes = 
				{
					new Node { Name = "C4" },
					new Node { Name = "C5" },
				}
			}, 
			new Node 
			{
				Name = "B3",
				Nodes = 
				{
					new Node { Name = "C6" },
				}
			}, 
		}
	};

	[Fact]
	public void WhenTraversingBreadthFirst_ThenReturnsInOrder()
	{
		var result = this.root
			.Traverse(TraverseKind.BreadthFirst, node => node.Nodes)
			.Select(node => node.Name)
			.ToList();

		Assert.Equal("A", result[0]);
		Assert.Equal("B3", result[1]);
		Assert.Equal("C6", result[2]);
		Assert.Equal("B2", result[3]);
		Assert.Equal("", result[4]);
		Assert.Equal("C2", result[5]);
		Assert.Equal("C3", result[6]);
		Assert.Equal("C4", result[7]);
		Assert.Equal("C5", result[8]);
		Assert.Equal("C6", result[9]);
	}

	[Fact]
	public void WhenTraversingDepthFirst_ThenReturnsInOrder()
	{
		var result = this.root
			.Traverse(TraverseKind.DepthFirst, node => node.Nodes)
			.Select(node => node.Name)
			.ToList();

		// Note: this order avoids having to reverse the child nodes list.
		Assert.Equal("A", result[0]);
		Assert.Equal("B3", result[1]);
		Assert.Equal("C6", result[2]);
		Assert.Equal("B2", result[3]);
		Assert.Equal("C5", result[4]);
		Assert.Equal("C4", result[5]);
		Assert.Equal("B1", result[6]);
		Assert.Equal("C3", result[7]);
		Assert.Equal("C2", result[8]);
		Assert.Equal("C1", result[9]);
	}

	public class Node
	{
		public Node()
		{
			this.Nodes = new List<Node>();
		}

		public string Name { get; set; }
		public List<Node> Nodes { get; set; }
	}
}
