/*
* Copyright 2015 Owen Bennett
* This file is a part of the Redirect-Launcher-MK2 by Owen Bennett.
* All code contained in here is licensed under the MIT license.
* Please fill issue report on http://git.potato.moe/potato-san/Redirect-Launcher-mk2
*/
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using MetroRadiance;
using System.IO;
using System.Reflection;

namespace RedirectLauncherMk2_WPF
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		private void Application_Startup(object sender, StartupEventArgs e)
		{
			ThemeService.Current.Initialize(this, Theme.Dark, Accent.Purple);

			FileInfo f = new FileInfo("launcherUpdate.exe");

			if (f != null && !System.AppDomain.CurrentDomain.FriendlyName.Equals("launcherUpdate.exe"))
				f.Delete();

			if (e.Args.Length > 0)
			{
				for (int i = 0; i < e.Args.Length; i++ )
				{
					if (e.Args[i].Equals("/u") && System.AppDomain.CurrentDomain.FriendlyName.Equals("launcherUpdate.exe"))
					{
						f.CopyTo(e.Args[i + 1]);
					}
				}
			}

			//Extract Servers.json if nonexistant
			if (!File.Exists("Servers.json"))
			{
				File.WriteAllBytes("Servers.json", RedirectLauncherMk2_WPF.Properties.Resources.Servers);
			}
			var main = new MainWindow();
			main.Show();
		}
	}
}
