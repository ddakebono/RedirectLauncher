/*
* Copyright 2015 Owen Bennett
* This file is a part of the Redirect-Launcher-MK2 by Owen Bennett.
* All code contained in here is licensed under the MIT license.
* Please fill issue report on http://git.potato.moe/potato-san/Redirect-Launcher-mk2
*/
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
		private List<Servers> serverList;
		private bool pageHasLoaded = false;

		public MainWindow()
		{
			InitializeComponent();
		}

		private void windowIsReady(object sender, EventArgs e)
		{

			client.loadNewPatchUrl(Properties.Settings.Default.DefaultPatchdata);
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
			WebBlock.Source = new Uri(client.launcherWebpage);
			TitleBlock.Text = client.launcherName;
		}

		private void LaunchGame(object sender, RoutedEventArgs e)
		{
			clientUpdater.checkClientUpdate(ProgressBar, ClientVersionBlock, StatusBlock, StatusPercentBlock);
			modUpdater.startModUpdate(ProgressBar, ClientVersionBlock, StatusBlock, StatusPercentBlock);
			if (client.clientVersion >= client.remoteClientVersion && ((client.clientModVersion >= client.remoteClientModVersion && modUpdater.doesModpackFileExist() && !modUpdater.isUpdateInProgress) || (modUpdater.hasUserSkippedUpdate)) && !clientUpdater.isUpdateInProgress)
			{
				client.LaunchGame();
			}
		}

		private void handleLinks(object sender, NavigatingCancelEventArgs e)
		{
			if (!pageHasLoaded)
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
			Options options = new Options(client);
			options.ShowDialog();
			reloadElements();
		}
	}
}
