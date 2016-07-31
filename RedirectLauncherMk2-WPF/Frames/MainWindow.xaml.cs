/*
* Copyright 2015 Owen Bennett
* This file is a part of the RedirectLauncher by Owen Bennett.
* All code contained in here is licensed under the MIT license.
* Please fill issue report on https://github.com/ripxfrostbite/RedirectLauncher
*/
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RedirectLauncherMk2_WPF.LauncherLogic;
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
		public LauncherSettings settings;
		public Game client;
		private ClientUpdater clientUpdater;
		private ModUpdater modUpdater;
		private SelfUpdater updater;
		public Serverlist serverList;
		private bool pageHasLoaded = false;

		public MainWindow()
		{
			settings = new LauncherSettings();
			client = new Game(settings);
			serverList = new Serverlist();
			InitializeComponent();
		}

		private void windowIsReady(object sender, EventArgs e)
		{
			client.loadNewPatchUrl((settings.selectedServer < serverList.servers.Count) ? serverList.servers[settings.selectedServer] : serverList.servers.First());
			reloadElements();
			updater = new SelfUpdater(client.launcherRepo, Properties.Settings.Default.Version, client.remoteLauncherVersion);
			updater.checkLauncherUpdates(ProgressBar, StatusBlock);
			clientUpdater = new ClientUpdater(client);
			modUpdater = new ModUpdater(client);
			reloadElements();
			StatusBlock.Text = "Ready to Launch!";
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
			TitleBlock.Text = client.launcherName;
			try
			{
				WebBlock.Source = new Uri(client.launcherWebpage);
			}
			catch (UriFormatException e)
			{
				System.Windows.Forms.MessageBox.Show("The URL given for Launcher Webpage was invalid!");
				WebBlock.Source = new Uri("about:blank");
			}
		}

		private void LaunchGame(object sender, RoutedEventArgs e)
		{
			settings.saveToRegistry();
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
			serverList.saveToFile();
			about.ShowDialog();
		}

		private void OpenOptionsWindow(object sender, RoutedEventArgs e)
		{
			Options options = new Options(client, serverList);
			options.ShowDialog();
			if (options.clientDirChanged)
			{
				client = new Game(settings);
			}
			if (options.selectedServerChanged || options.clientDirChanged)
			{
				client.loadNewPatchUrl(serverList.servers[settings.selectedServer]);
			}
			reloadElements();
		}
	}
}
