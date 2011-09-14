#region BSD License
/* 
Copyright (c) 2011, NETFx
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, 
are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this list 
  of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice, this 
  list of conditions and the following disclaimer in the documentation and/or other 
  materials provided with the distribution.

* Neither the name of Clarius Consulting nor the names of its contributors may be 
  used to endorse or promote products derived from this software without specific 
  prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY 
EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES 
OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT 
SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, 
INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED 
TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR 
BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN 
ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH 
DAMAGE.
*/
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.ComponentModel;

namespace NetFx.System.ComponentModel
{
	public class ComponentModelAttributesSpec
	{
		[Fact]
		public void WhenRetrievingAttributes_ThenSucceeds()
		{
			Assert.Equal(true, Reflect<Target>.GetProperty(x => x.BindableTrue).ComponentModel().Bindable);
			Assert.Equal(false, Reflect<Target>.GetProperty(x => x.BindableFalse).ComponentModel().Bindable);
			Assert.Equal(true, Reflect<Target>.GetProperty(x => x.BindableYes).ComponentModel().Bindable);
			Assert.Equal(false, Reflect<Target>.GetProperty(x => x.BindableNo).ComponentModel().Bindable);
			Assert.Equal(null, Reflect<Target>.GetProperty(x => x.NoBindable).ComponentModel().Bindable);
			Assert.Equal(BindingDirection.OneWay, Reflect<Target>.GetProperty(x => x.OneWayBindable).ComponentModel().BindingDirection);
			Assert.Equal(BindingDirection.TwoWay, Reflect<Target>.GetProperty(x => x.TwoWayBindable).ComponentModel().BindingDirection);

			Assert.Equal("Foo", Reflect<Target>.GetProperty(x => x.Category).ComponentModel().Category);
			Assert.Equal(null, Reflect<Target>.GetProperty(x => x.NoCategory).ComponentModel().Category);

			Assert.Equal("Foo", Reflect<Target>.GetProperty(x => x.Description).ComponentModel().Description);
			Assert.Equal(null, Reflect<Target>.GetProperty(x => x.NoDescription).ComponentModel().Description);

			Assert.Equal("Foo", Reflect<Target>.GetProperty(x => x.DisplayName).ComponentModel().DisplayName);
			Assert.Equal(null, Reflect<Target>.GetProperty(x => x.NoDisplayName).ComponentModel().DisplayName);

			Assert.Equal("Foo", (string)Reflect<Target>.GetProperty(x => x.AmbientValue).ComponentModel().AmbientValue);
			Assert.Equal(null, Reflect<Target>.GetProperty(x => x.NoAmbientValue).ComponentModel().AmbientValue);

			Assert.Equal("Foo", Reflect<Target>.GetProperty(x => x.AttributeProvider).ComponentModel().AttributeProvider.TypeName);
			Assert.Equal("Bar", Reflect<Target>.GetProperty(x => x.AttributeProvider).ComponentModel().AttributeProvider.PropertyName);
			Assert.Equal(null, Reflect<Target>.GetProperty(x => x.NoAttributeProvider).ComponentModel().AttributeProvider);

			Assert.Equal(true, Reflect<Target>.GetProperty(x => x.BrowsableTrue).ComponentModel().Browsable);
			Assert.Equal(false, Reflect<Target>.GetProperty(x => x.BrowsableFalse).ComponentModel().Browsable);
			Assert.Equal(null, Reflect<Target>.GetProperty(x => x.NoBrowsable).ComponentModel().Browsable);

			Assert.Equal("Source", typeof(ComplexBinding).ComponentModel().ComplexBindingDataSource);
			Assert.Equal("Member", typeof(ComplexBinding).ComponentModel().ComplexBindingDataMember);
			Assert.Equal(null, typeof(NoComplexBinding).ComponentModel().ComplexBindingDataSource);
			Assert.Equal(null, typeof(NoComplexBinding).ComponentModel().ComplexBindingDataMember);

			Assert.NotNull(Reflect<Target>.GetProperty(x => x.AttributeProvider).ComponentModel().AttributeProvider.TypeName);
			Assert.Equal("Bar", Reflect<Target>.GetProperty(x => x.AttributeProvider).ComponentModel().AttributeProvider.PropertyName);
			Assert.Equal(null, Reflect<Target>.GetProperty(x => x.NoAttributeProvider).ComponentModel().AttributeProvider);

			Assert.Equal("Foo", (string)Reflect<Target>.GetProperty(x => x.DefaultValue).ComponentModel().DefaultValue);
			Assert.Equal(null, Reflect<Target>.GetProperty(x => x.NoDefaultValue).ComponentModel().DefaultValue);

			Assert.Equal(true, Reflect<Target>.GetProperty(x => x.ReadOnlyTrue).ComponentModel().IsReadOnly);
			Assert.Equal(false, Reflect<Target>.GetProperty(x => x.ReadOnlyFalse).ComponentModel().IsReadOnly);
			Assert.Equal(null, Reflect<Target>.GetProperty(x => x.NoReadOnly).ComponentModel().IsReadOnly);

			Assert.Equal(true, Reflect<Target>.GetProperty(x => x.DesignOnlyTrue).ComponentModel().IsDesignOnly);
			Assert.Equal(false, Reflect<Target>.GetProperty(x => x.DesignOnlyFalse).ComponentModel().IsDesignOnly);
			Assert.Equal(null, Reflect<Target>.GetProperty(x => x.NoDesignOnly).ComponentModel().IsDesignOnly);

			//Assert.Equal(DesignerSerializationVisibility.Content, Reflect<Target>.GetProperty(x => x.DesignerSerializationVisibilityContent).ComponentModel().DesignerSerializationVisibility);
			//Assert.Equal(DesignerSerializationVisibility.Hidden, Reflect<Target>.GetProperty(x => x.DesignerSerializationVisibilityHidden).ComponentModel().DesignerSerializationVisibility);
			//Assert.Equal(DesignerSerializationVisibility.Visible, Reflect<Target>.GetProperty(x => x.DesignerSerializationVisibilityVisible).ComponentModel().DesignerSerializationVisibility);
			//Assert.Equal(null, Reflect<Target>.GetProperty(x => x.DesignerSerializationVisibilityVisible).ComponentModel().DesignerSerializationVisibility);

			Assert.Equal(EditorBrowsableState.Advanced, Reflect<Target>.GetProperty(x => x.EditorBrowsableAdvanced).ComponentModel().EditorBrowsable);
			Assert.Equal(EditorBrowsableState.Always, Reflect<Target>.GetProperty(x => x.EditorBrowsableAlways).ComponentModel().EditorBrowsable);
			Assert.Equal(EditorBrowsableState.Never, Reflect<Target>.GetProperty(x => x.EditorBrowsableNever).ComponentModel().EditorBrowsable);
			Assert.Equal(null, Reflect<Target>.GetProperty(x => x.NoEditorBrowsable).ComponentModel().EditorBrowsable);

			Assert.Equal(true, Reflect<Target>.GetProperty(x => x.ImmutableTrue).ComponentModel().IsImmutable);
			Assert.Equal(false, Reflect<Target>.GetProperty(x => x.ImmutableFalse).ComponentModel().IsImmutable);
			Assert.Equal(null, Reflect<Target>.GetProperty(x => x.NoImmutable).ComponentModel().IsImmutable);

			Assert.Equal(true, Reflect<Target>.GetProperty(x => x.LocalizableTrue).ComponentModel().IsLocalizable);
			Assert.Equal(false, Reflect<Target>.GetProperty(x => x.LocalizableFalse).ComponentModel().IsLocalizable);
			Assert.Equal(null, Reflect<Target>.GetProperty(x => x.NoLocalizable).ComponentModel().IsLocalizable);

			Assert.Equal(true, Reflect<Target>.GetProperty(x => x.SettingsBindableTrue).ComponentModel().SettingsBindable);
			Assert.Equal(false, Reflect<Target>.GetProperty(x => x.SettingsBindableFalse).ComponentModel().SettingsBindable);
			Assert.Equal(null, Reflect<Target>.GetProperty(x => x.NoSettingsBindable).ComponentModel().SettingsBindable);

		}

		public class Target
		{
			[Bindable(BindableSupport.Yes, BindingDirection.OneWay)]
			public int OneWayBindable { get; set; }

			[Bindable(BindableSupport.Yes, BindingDirection.TwoWay)]
			public int TwoWayBindable { get; set; }

			public int NoBindable { get; set; }

			[Bindable(BindableSupport.Yes)]
			public int BindableYes { get; set; }

			[Bindable(BindableSupport.No)]
			public int BindableNo { get; set; }

			[Bindable(true)]
			public int BindableTrue { get; set; }

			[Bindable(false)]
			public int BindableFalse { get; set; }

			[Category("Foo")]
			public int Category { get; set; }

			public int NoCategory { get; set; }

			[Description("Foo")]
			public int Description { get; set; }

			public int NoDescription { get; set; }

			[DisplayName("Foo")]
			public int DisplayName { get; set; }

			public int NoDisplayName { get; set; }

			[AmbientValue("Foo")]
			public int AmbientValue { get; set; }

			public int NoAmbientValue { get; set; }

			[AttributeProvider("Foo", "Bar")]
			public int AttributeProvider { get; set; }

			public int NoAttributeProvider { get; set; }

			[Browsable(true)]
			public int BrowsableTrue { get; set; }

			[Browsable(false)]
			public int BrowsableFalse { get; set; }

			public int NoBrowsable { get; set; }

			[DefaultValue("Foo")]
			public int DefaultValue { get; set; }

			public int NoDefaultValue { get; set; }

			[ReadOnly(true)]
			public int ReadOnlyTrue { get; set; }

			[ReadOnly(false)]
			public int ReadOnlyFalse { get; set; }

			public int NoReadOnly { get; set; }

			[DesignOnly(true)]
			public int DesignOnlyTrue { get; set; }

			[DesignOnly(false)]
			public int DesignOnlyFalse { get; set; }

			public int NoDesignOnly { get; set; }

			//[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
			//public int DesignerSerializationVisibilityContent { get; set; }

			//[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
			//public int DesignerSerializationVisibilityHidden { get; set; }

			//[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
			//public int DesignerSerializationVisibilityVisible { get; set; }

			//public int NoDesignerSerializationVisibility { get; set; }

			[EditorBrowsable(EditorBrowsableState.Advanced)]
			public int EditorBrowsableAdvanced { get; set; }

			[EditorBrowsable(EditorBrowsableState.Always)]
			public int EditorBrowsableAlways { get; set; }

			[EditorBrowsable(EditorBrowsableState.Never)]
			public int EditorBrowsableNever { get; set; }

			public int NoEditorBrowsable { get; set; }

			[ImmutableObject(true)]
			public int ImmutableTrue { get; set; }

			[ImmutableObject(false)]
			public int ImmutableFalse { get; set; }

			public int NoImmutable { get; set; }

			[Localizable(true)]
			public int LocalizableTrue { get; set; }

			[Localizable(false)]
			public int LocalizableFalse { get; set; }

			public int NoLocalizable { get; set; }

			[SettingsBindable(true)]
			public int SettingsBindableTrue { get; set; }

			[SettingsBindable(false)]
			public int SettingsBindableFalse { get; set; }

			public int NoSettingsBindable { get; set; }
		}

		[ComplexBindingProperties("Source", "Member")]
		public class ComplexBinding { }

		public class NoComplexBinding { }
	}
}