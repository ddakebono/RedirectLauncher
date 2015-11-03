using MetroRadiance.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RedirectLauncherMk2_WPF
{
	/// <summary>
	/// Interaction logic for Options.xaml
	/// </summary>
	public partial class Options : MetroWindow
	{
		public Game client;

		public Options(Game client)
		{
			this.client = client;
			InitializeComponent();
		}

		private void Apply(object sender, RoutedEventArgs e)
		{

		}

		private void Cancel(object sender, RoutedEventArgs e)
		{
			this.Close();
		}
	}
}
