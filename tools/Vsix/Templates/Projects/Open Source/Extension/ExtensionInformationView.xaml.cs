using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NetFx.Templates.Projects.OpenSource.Extension
{
	/// <summary>
	/// Interaction logic for ExtensionInformation.xaml
	/// </summary>
	public partial class ExtensionInformationView : Window
	{
		public ExtensionInformationView()
		{
			InitializeComponent();

			this.Model = new ExtensionInformationModel();
			this.DataContext = this.Model;
		}

		public ExtensionInformationModel Model { get; private set; }

		private void Accept(object sender, RoutedEventArgs e)
		{
			this.DialogResult = true;
			Close();
		}
	}
}
