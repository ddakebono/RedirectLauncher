/*
* Copyright 2015 Owen Bennett
* This file is a part of the RedirectLauncher by Owen Bennett.
* All code contained in here is licensed under the MIT license.
* Please fill issue report on https://github.com/ripxfrostbite/RedirectLauncher
*/
using MahApps.Metro.Controls;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace RedirectLauncherMk2_WPF
{
	/// <summary>
	/// Interaction logic for Options.xaml
	/// </summary>
	public partial class Options : MetroWindow
	{
		private Game client;
		private Serverlist serverList;
		public bool clientDirChanged;
		public bool selectedServerChanged;

		public Options(Game client, Serverlist serverList)
		{
			this.client = client;
			this.serverList = serverList;
			InitializeComponent();
		}

		private void ApplyClick(object sender, RoutedEventArgs e)
		{
			client.settings.clientInstallDirectory = ClientDir.Text;
			client.settings.selectedServer = ServerList.SelectedIndex;
			client.settings.launchKanan = (bool)Kanan.IsChecked;
			client.settings.launchDevTools = (bool)Proxy.IsChecked;
			client.settings.kananFolder = FolderSelect.Text;
			client.settings.saveToRegistry();
			this.Close();
		}

		private void CancelClick(object sender, RoutedEventArgs e)
		{
			clientDirChanged = false;
			selectedServerChanged = false;
			this.Close();
		}

		private void WindowIsLoaded(object sender, RoutedEventArgs e)
		{
			ServerList.ItemsSource = serverList.servers;
			ServerList.DisplayMemberPath = "name";
			ServerList.SelectedIndex = serverList.servers.FindIndex(x => x.name.Equals(client.selectedServer.name));
			Kanan.IsChecked = client.settings.launchKanan;
			Proxy.IsChecked = client.settings.launchDevTools;
			FolderSelect.Text = client.settings.kananFolder;
			ClientDir.Text = client.settings.clientInstallDirectory;
			FolderSelect.ToolTip = client.settings.kananFolder;
			ClientDir.ToolTip = client.settings.clientInstallDirectory;
			clientVersion.Value = client.clientVersion;
			clientDirChanged = false;
			selectedServerChanged = false;
			
		}

		private void OpenFolderBrowser(object sender, RoutedEventArgs e)
		{
			FolderBrowserDialog find = new FolderBrowserDialog();
			find.Description = "Please select the folder that contains the kanan.py file.";
			DialogResult selection = find.ShowDialog();
			String directory = find.SelectedPath;
			if (File.Exists(directory + "\\kanan.py"))
			{
				//User defined folder has required data
				FolderSelect.Text = directory;
			}
			else
			{
				System.Windows.Forms.MessageBox.Show("The selected folder doesn't contain the kanan.py, please choose another one.");
			}

		}

		private void SelectClientFolder(object sender, RoutedEventArgs e)
		{
			FolderBrowserDialog find = new FolderBrowserDialog();
			find.Description = "Select the Mabinogi Client Directory.\nIf one doesn't exist just choose anywhere, the launcher will start a full download.";
			DialogResult selection = find.ShowDialog();
			String directory = find.SelectedPath;
			if (File.Exists(directory + "\\version.dat"))
			{
				//User defined folder has required data
				ClientDir.Text = directory;
				clientDirChanged = true;
			}
			else
			{
				System.Windows.Forms.MessageBox.Show("This doesn't seem to be a client folder, if you launch with this set a full game download will begin.");
			}
		}

		private void changeServer(object sender, SelectionChangedEventArgs e)
		{
			selectedServerChanged = true;
		}

		private void OpenManageServer(object sender, RoutedEventArgs e)
		{
			ServerManager manager = new ServerManager(serverList.servers[ServerList.SelectedIndex]);
			manager.ShowDialog();
			selectedServerChanged = true;
			serverList.saveToFile();
		}

		private void OpenAddServer(object sender, RoutedEventArgs e)
		{
			ServerManager manager = new ServerManager();
			manager.ShowDialog();
			if (manager.server != null)
			{
				serverList.servers.Add(manager.server);
				ServerList.ItemsSource = serverList.servers;
				ServerList.SelectedIndex = serverList.servers.IndexOf(manager.server);
			}
			serverList.saveToFile();
		}

		private void ApplyVersionChange(object sender, RoutedEventArgs e)
		{
			if (System.Windows.MessageBox.Show("You're about to change your version.dat to a different number, this will make the launcher update to the latest version again. Are you sure?", "Change Version", MessageBoxButton.YesNo, MessageBoxImage.Asterisk) == MessageBoxResult.Yes)
			{
				if (clientVersion.Value.HasValue)
				{
					client.writeVersionData(clientVersion.Value.Value, null);
					selectedServerChanged = true;
				}
			}
		}
	}
}
