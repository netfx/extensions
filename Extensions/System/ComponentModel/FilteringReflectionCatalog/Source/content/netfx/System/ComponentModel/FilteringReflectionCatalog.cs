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
using System.ComponentModel.Composition.Primitives;
using System.ComponentModel.Composition.ReflectionModel;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition;

/// <nuget id="netfx-System.ComponentModel.FilteringReflectionCatalog" />
internal class FilteringReflectionCatalog : ComposablePartCatalog, ICompositionElement
{
	private bool initialized;
	private List<ComposablePartDefinition> sharedParts = new List<ComposablePartDefinition>();
	private List<ComposablePartDefinition> nonSharedParts = new List<ComposablePartDefinition>();
	private readonly ComposablePartCatalog innerCatalog;

	/// <summary>
	/// Argument passed to the export filter, containing 
	/// detailed information about the export.
	/// </summary>
	public class FilteredExport
	{
		/// <summary>
		/// Initializes the context from a part and the export.
		/// </summary>
		internal FilteredExport(ComposablePartDefinition part, ExportDefinition export)
		{
			this.ExportDefinition = export;
			this.ExportingMember = ReflectionModelServices.GetExportingMember(export);
			this.ExportingType = ReflectionModelServices.GetPartType(part).Value;
		}

		/// <summary>
		/// Gets the original export definition.
		/// </summary>
		public ExportDefinition ExportDefinition { get; private set; }
		/// <summary>
		/// Gets the type that provides the export.
		/// </summary>
		public Type ExportingType { get; private set; }
		/// <summary>
		/// Optional member where the export is provided.
		/// </summary>
		public LazyMemberInfo ExportingMember { get; private set; }
	}

	/// <summary>
	/// Argument passed to the part filter, containing
	/// detailed information about the part definition.
	/// </summary>
	public class FilteredPart
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="FilteredPart"/> class with the 
		/// given part definition.
		/// </summary>
		public FilteredPart(ComposablePartDefinition part)
		{
			this.PartDefinition = part;
			this.PartType = ReflectionModelServices.GetPartType(part).Value;
		}

		/// <summary>
		/// Gets the part definition.
		/// </summary>
		public ComposablePartDefinition PartDefinition { get; private set; }

		/// <summary>
		/// Gets the concrete type of the part.
		/// </summary>
		public Type PartType { get; private set; }
	}

	/// <summary>
	/// Initializes the catalog.
	/// </summary>
	public FilteringReflectionCatalog(ComposablePartCatalog innerCatalog)
	{
		this.innerCatalog = innerCatalog;
		this.PartFilter = context => true;
		this.ExportFilter = context => true;
	}

	/// <summary>
	/// Gets or sets the filter for part definitions.
	/// </summary>
	public Func<FilteredPart, bool> PartFilter { get; set; }

	/// <summary>
	/// Gets or sets the filter for exports.
	/// </summary>
	public Func<FilteredExport, bool> ExportFilter { get; set; }

	/// <summary>
	/// Gets the filtered exports from the inner catalog.
	/// </summary>
	public override IEnumerable<Tuple<ComposablePartDefinition, ExportDefinition>> GetExports(ImportDefinition definition)
	{
		return base.GetExports(definition).Where(tuple => ExportFilter(new FilteredExport(tuple.Item1, tuple.Item2)));
	}

	/// <summary>
	/// Gets the filtered parts.
	/// </summary>
	public override IQueryable<ComposablePartDefinition> Parts
	{
		get
		{
			if (!initialized)
				Initialize(this.innerCatalog.Parts);

			return this.sharedParts.Concat(CloneNonSharedParts()).AsQueryable();
		}
	}

	/// <summary>
	/// Clones the non-shared to avoid object instance reuse, 
	/// which happens if you cache the part definition.
	/// </summary>
	private IEnumerable<ComposablePartDefinition> CloneNonSharedParts()
	{
		return this.nonSharedParts
			.AsParallel()
			.Where(part => part != null)
			.Select(def => ReflectionModelServices.CreatePartDefinition(
				ReflectionModelServices.GetPartType(def),
				true,
				new Lazy<IEnumerable<ImportDefinition>>(() => def.ImportDefinitions),
				new Lazy<IEnumerable<ExportDefinition>>(() => def.ExportDefinitions),
				new Lazy<IDictionary<string, object>>(() => def.Metadata),
				this));
	}

	private void Initialize(IQueryable<ComposablePartDefinition> parts)
	{
		var partsInfo = parts
			.AsParallel()
			.Where(part => part != null && this.PartFilter(new FilteredPart(part)))
			.Select(part => new { Part = part, IsShared = IsShared(part) });

		sharedParts.AddRange(partsInfo.Where(part => part.IsShared).Select(part => part.Part));
		nonSharedParts.AddRange(partsInfo.Where(part => !part.IsShared).Select(part => part.Part));

		initialized = true;
	}

	private static bool IsShared(ComposablePartDefinition def)
	{
		return def.Metadata.ContainsKey(CompositionConstants.PartCreationPolicyMetadataName) &&
			(CreationPolicy)def.Metadata[CompositionConstants.PartCreationPolicyMetadataName] == CreationPolicy.Shared;
	}

	/// <summary>
	/// Disposes the inner container.
	/// </summary>
	protected override void Dispose(bool disposing)
	{
		if (disposing)
			innerCatalog.Dispose();
	}

	public string DisplayName
	{
		get { return "Filtered catalog"; }
	}

	public ICompositionElement Origin
	{
		get { return null; }
	}
}
