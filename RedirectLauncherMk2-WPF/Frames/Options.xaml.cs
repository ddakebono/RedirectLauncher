/*
* Copyright 2015 Owen Bennett
* This file is a part of the RedirectLauncher by Owen Bennett.
* All code contained in here is licensed under the MIT license.
* Please fill issue report on https://github.com/ripxfrostbite/RedirectLauncher
*/
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
		private Game client;
		private List<Server> serverList;

		public Options(Game client, List<Server> serverList)
		{
			this.client = client;
			this.serverList = serverList;
			InitializeComponent();
		}

		private void ApplyClick(object sender, RoutedEventArgs e)
		{
			client.loadNewPatchUrl(serverList[ServerList.SelectedIndex]);
			client.launchCrackShield = (bool)HShield.IsChecked;
			client.launchDevTools = (bool)Proxy.IsChecked;
			client.customLoginServer = CustomLogip.Text;
			this.Close();
		}

		private void CancelClick(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

		private void WindowIsLoaded(object sender, RoutedEventArgs e)
		{
			ServerList.ItemsSource = serverList;
			ServerList.DisplayMemberPath = "name";
			ServerList.SelectedIndex = serverList.FindIndex(x => x.name.Equals(client.selectedServer.name));
			HShield.IsChecked = client.launchCrackShield;
			Proxy.IsChecked = client.launchDevTools;
		}
	}
}
