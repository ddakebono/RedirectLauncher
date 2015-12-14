using MetroRadiance.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
	/// Interaction logic for About.xaml
	/// </summary>
	public partial class About : MetroWindow
	{
		public About()
		{
			InitializeComponent();
			this.DataContext = new TextBlock() { Text = Properties.Settings.Default.Version.ToString() };
			
		}

		private void HyperlinkClik(object sender, RoutedEventArgs e)
		{
			Process.Start("http://git.potato.moe/potato-san/Redirect-Launcher-mk2/");
		}

	}
}
