/*
* Copyright 2015 Owen Bennett
* This file is a part of the Redirect-Launcher-MK2 by Owen Bennett.
* All code contained in here is licensed under the MIT license.
* Please fill issue report on http://git.potato.moe/potato-san/Redirect-Launcher-mk2
*/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace RedirectLauncherMk2_WPF
{
	class SelfUpdater
	{
		private String launcherRepoUrl;
		private int localLauncherVersion;
		private int remoteLauncherVersion;
		private ProgressBar progressBar;
		private TextBlock statusBlock;

		public SelfUpdater(String launcherRepoUrl, int localLauncherVersion, int remoteLauncherVersion)
		{
			this.launcherRepoUrl = launcherRepoUrl;
			this.localLauncherVersion = localLauncherVersion;
			this.remoteLauncherVersion = remoteLauncherVersion;
		}

		public void checkLauncherUpdates(ProgressBar progressBar, TextBlock statusBlock)
		{
			this.progressBar = progressBar;
			this.statusBlock = statusBlock;
			if (localLauncherVersion < remoteLauncherVersion)
			{
				statusBlock.Text = "Updating the Launcher (0%)";
				downloadLauncher(remoteLauncherVersion);
			}
			else
			{
				progressBar.Value = 100;
			}
		}

		private void downloadLauncher(int version)
		{
			WebClient w = new WebClient();
			w.DownloadFileCompleted += new AsyncCompletedEventHandler(downloadComplete);
			w.DownloadProgressChanged += new DownloadProgressChangedEventHandler(progressUpdate);
			w.DownloadFileAsync(new Uri(launcherRepoUrl + "launcher-" + version + ".exe"), @"launcherUpdate.tmp");
		}

		//Async download functions
		private void downloadComplete(object sender, AsyncCompletedEventArgs e)
		{
			FileInfo f = new FileInfo("launcherUpdate.tmp");
			if (f.Length > 0)
			{
				ProcessStartInfo p = new ProcessStartInfo();
				p.Arguments = "/C choice /C Y /N /D Y /T 3 & del " + System.Reflection.Assembly.GetEntryAssembly().Location + " & rename launcherUpdate.tmp " + System.AppDomain.CurrentDomain.FriendlyName + "& START " + System.Reflection.Assembly.GetEntryAssembly().Location;
				p.WindowStyle = ProcessWindowStyle.Hidden;
				p.CreateNoWindow = true;
				p.FileName = "cmd.exe";
				Process.Start(p);
				System.Environment.Exit(0);
			}
			else
			{
				f.Delete();
				MessageBox.Show("A launcher update failed to be downloaded, please try again later...");
			}
		}
		private void progressUpdate(object sender, DownloadProgressChangedEventArgs e)
		{
			progressBar.Value = e.ProgressPercentage;
			statusBlock.Text = "Updating the Launcher (" + e.ProgressPercentage + "%)";
		}
	}
}
