/*
* Copyright 2015 Owen Bennett
* This file is a part of the RedirectLauncher by Owen Bennett.
* All code contained in here is licensed under the MIT license.
* Please fill issue report on https://github.com/ripxfrostbite/RedirectLauncher
*/
using MetroRadiance.Controls;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace RedirectLauncherMk2_WPF
{
	/// <summary>
	/// Interaction logic for Options.xaml
	/// </summary>
	public partial class Options : MetroWindow
	{
		private Game client;
		private List<Server> serverList;
		public bool clientDirChanged;
		public bool selectedServerChanged;

		public Options(Game client, List<Server> serverList)
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
			client.settings.customLoginIP = CustomLogip.Text;
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
			ServerList.ItemsSource = serverList;
			ServerList.DisplayMemberPath = "name";
			ServerList.SelectedIndex = serverList.FindIndex(x => x.name.Equals(client.selectedServer.name));
			Kanan.IsChecked = client.settings.launchKanan;
			Proxy.IsChecked = client.settings.launchDevTools;
			FolderSelect.Text = client.settings.kananFolder;
			ClientDir.Text = client.settings.clientInstallDirectory;
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
	}
}
