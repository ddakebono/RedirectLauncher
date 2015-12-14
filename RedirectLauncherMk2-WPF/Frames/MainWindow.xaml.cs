using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
/*
* Copyright 2015 Owen Bennett
* This file is a part of the Redirect-Launcher-MK2 by Owen Bennett.
* All code contained in here is licensed under the MIT license.
* Please fill issue report on http://git.potato.moe/potato-san/Redirect-Launcher-mk2
*/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RedirectLauncherMk2_WPF
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		public Game client = new Game();
		private ClientUpdater clientUpdater;
		private ModUpdater modUpdater;
		private SelfUpdater updater;
		private List<Server> serverList;
		private bool pageHasLoaded = false;

		public MainWindow()
		{
			serverList = loadServerList();
			InitializeComponent();
		}

		private void windowIsReady(object sender, EventArgs e)
		{

			client.loadNewPatchUrl(serverList.First());
			reloadElements();
			updater = new SelfUpdater(client.launcherRepo, Properties.Settings.Default.Version, client.remoteLauncherVersion);
			updater.checkLauncherUpdates(ProgressBar, StatusBlock);
			clientUpdater = new ClientUpdater(client);
			modUpdater = new ModUpdater(client);
			reloadElements();
			StatusBlock.Text = "Ready to Launch!";
		}

		private List<Server> loadServerList()
		{
			List<Server> serverList = new List<Server>();
			StreamReader jsonFile = File.OpenText("Servers.json");
			JsonTextReader reader = new JsonTextReader(jsonFile);
			JsonSerializer json = new JsonSerializer();
			var servers = json.Deserialize<JArray>(reader);
			foreach (var server in servers)
			{
				serverList.Add(new Server(server.Value<string>("name"), server.Value<string>("patchdata"), server.Value<string>("launcherPage"), server.Value<int>("resWidth"), server.Value<int>("resHeight")));
			}
			return serverList;
		}

		public void reloadElements()
		{
			ClientVersionBlock.Text = client.getLocalClientVersionString();
			LauncherVersionBlock.Text = Properties.Settings.Default.Version.ToString();
			RemoteClientVersionBlock.Text = client.getRemoteClientVersionString();
			RemoteLauncherVersionBlock.Text = client.remoteLauncherVersion.ToString();
			pageHasLoaded = false;
			if (client.selectedServer.resWidth > 0)
				WebBlock.Width = client.selectedServer.resWidth;
			else
				WebBlock.Width = Double.NaN;
			if (client.selectedServer.resHeight > 0)
				WebBlock.Height = client.selectedServer.resHeight;
			else
				WebBlock.Height = Double.NaN;
			WebBlock.Source = new Uri(client.launcherWebpage);
			TitleBlock.Text = client.launcherName;
		}

		private void LaunchGame(object sender, RoutedEventArgs e)
		{
			clientUpdater.checkClientUpdate(ProgressBar, ClientVersionBlock, StatusBlock, StatusPercentBlock);
			modUpdater.startModUpdate(ProgressBar, ClientVersionBlock, StatusBlock, StatusPercentBlock);
			if (client.clientVersion >= client.remoteClientVersion && ((client.clientModVersion >= client.remoteClientModVersion && modUpdater.doesModpackFileExist(client.clientModVersion) && !modUpdater.isUpdateInProgress) || (modUpdater.hasUserSkippedUpdate)) && !clientUpdater.isUpdateInProgress)
			{
				client.LaunchGame();
			}
		}

		private void handleLinks(object sender, NavigatingCancelEventArgs e)
		{
			if (!pageHasLoaded || e.Uri.Equals(client.selectedServer.launcherPage))
			{
				pageHasLoaded = true;
				return;
			}
			e.Cancel = true;
			Process.Start(e.Uri.ToString());
		}

		private void OpenAboutWindow(object sender, RoutedEventArgs e)
		{
			About about = new About();
			about.ShowDialog();
		}

		private void OpenOptionsWindow(object sender, RoutedEventArgs e)
		{
			Options options = new Options(client, serverList);
			options.ShowDialog();
			reloadElements();
		}
	}
}
