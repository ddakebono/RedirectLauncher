/*
* Copyright 2015 Owen Bennett
* This file is a part of the RedirectLauncher by Owen Bennett.
* All code contained in here is licensed under the MIT license.
* Please fill issue report on https://github.com/ripxfrostbite/RedirectLauncher
*/
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using RedirectLauncherMk2_WPF.LauncherLogic;
using RedirectLauncherMk2_WPF.Updater;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Navigation;

namespace RedirectLauncherMk2_WPF
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : MetroWindow
	{
		public LauncherSettings settings;
		public Game client;
		private ClientUpdaterNew clientUpdater;
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
			clientUpdater = new ClientUpdaterNew(client, this);
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
			this.Title = client.launcherName;
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
			clientUpdater.loadManifestForVersion(client.remoteClientVersion);
			clientUpdater.getInstallDiff();
			//clientUpdater.checkClientUpdate(ProgressBar, ClientVersionBlock, StatusBlock, StatusPercentBlock);
			/*modUpdater.startModUpdate(ProgressBar, ClientVersionBlock, StatusBlock, StatusPercentBlock);
			if (client.clientVersion >= client.remoteClientVersion && ((client.clientModVersion >= client.remoteClientModVersion && modUpdater.doesModpackFileExist(client.clientModVersion) && !modUpdater.isUpdateInProgress) || (modUpdater.hasUserSkippedUpdate)) && !clientUpdater.isUpdateInProgress)
			{
				client.LaunchGame();
			}*/
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

		public async void displayAlertDialog(string title, string message)
		{
			WebBlock.Visibility = Visibility.Hidden;
			await this.ShowMessageAsync(title, message);
			WebBlock.Visibility = Visibility.Visible;
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
