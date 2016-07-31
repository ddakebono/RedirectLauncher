/*
* Copyright 2015 Owen Bennett
* This file is a part of the RedirectLauncher by Owen Bennett.
* All code contained in here is licensed under the MIT license.
* Please fill issue report on https://github.com/ripxfrostbite/RedirectLauncher
*/
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

		private void OpenGithub(object sender, RoutedEventArgs e)
		{
			Process.Start("https://github.com/ripxfrostbite/RedirectLauncher");
		}

	}
}
