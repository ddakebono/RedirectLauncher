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
	/// Interaction logic for ServerManager.xaml
	/// </summary>
	public partial class ServerManager : MetroWindow
	{
		public Server server;

		public ServerManager(Server server = null)
		{
			InitializeComponent();
			this.server = server;
			if (server != null)
			{
				ServerName.IsEnabled = false;
				ServerName.Text = server.name;
				PatchdataURL.Text = server.patchdata;
				if (server.launcherPage != null)
				{
					LauncherWebpage.Text = server.launcherPage;
				}
				if (server.patchdataOverride != null && server.patchdataOverride.Count > 0)
				{
					try
					{
						Version.Text = server.patchdataOverride["main_version"];
						LoginIP.Text = server.patchdataOverride["login"];
						Arguments.Text = server.patchdataOverride["arg"];
						PackageServer.Text = server.patchdataOverride["main_ftp"];
						LoginPort.Text = server.patchdataOverride["login_port"];
						ModVersion.Text = server.patchdataOverride["redirect_mod_version"];
						ModPackRepo.Text = server.patchdataOverride["redirect_mod_repo"];
					}
					catch (KeyNotFoundException e)
					{
					}
				}
			}
		}

		private void SaveChanges(object sender, RoutedEventArgs e)
		{
			if (server == null)
			{
				server = new Server();
				server.name = ServerName.Text;
			}

			server.patchdataOverride = new Dictionary<string, string>();

			server.patchdata = PatchdataURL.Text;
			server.launcherPage = LauncherWebpage.Text;

			if (Version.Text.Length > 0)
				server.patchdataOverride.Add("main_version", Version.Text);
			if (PackageServer.Text.Length > 0)
				server.patchdataOverride.Add("main_ftp", PackageServer.Text);
			if (LoginIP.Text.Length > 0)
				server.patchdataOverride.Add("login", LoginIP.Text);
			if (LoginPort.Text.Length > 0)
				server.patchdataOverride.Add("login_port", LoginPort.Text);
			if (Arguments.Text.Length > 0)
				server.patchdataOverride.Add("arg", Arguments.Text);
			if (ModPackRepo.Text.Length > 0)
				server.patchdataOverride.Add("redirect_mod_repo", ModPackRepo.Text);
			if (ModVersion.Text.Length > 0)
				server.patchdataOverride.Add("redirect_mod_version", ModVersion.Text);

			Close();
		}

		private void Cancel(object sender, RoutedEventArgs e)
		{
			server = null;
			Close();
		}
	}
}
