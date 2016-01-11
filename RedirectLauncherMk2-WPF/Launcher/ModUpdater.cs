﻿/*
* Copyright 2015 Owen Bennett
* This file is a part of the RedirectLauncher by Owen Bennett.
* All code contained in here is licensed under the MIT license.
* Please fill issue report on https://github.com/ripxfrostbite/RedirectLauncher
*/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace RedirectLauncherMk2_WPF
{
	class ModUpdater
	{
		private Game client;
		private ProgressBar progressBar;
		private TextBlock clientVersionBlock;
		public bool isUpdateInProgress = false;
		public bool hasUserSkippedUpdate = false;
		private TextBlock statusBlock;
		private TextBlock statusPercentBlock;
		private string serverModpack;


		public ModUpdater(Game client)
		{
			this.client = client;
		}

		public bool startModUpdate(ProgressBar progressBar, TextBlock clientVersionBlock, TextBlock statusBlock, TextBlock statusPercentBlock)
		{
			this.statusBlock = statusBlock;
			this.statusPercentBlock = statusPercentBlock;
			this.progressBar = progressBar;
			this.clientVersionBlock = clientVersionBlock;
			isUpdateInProgress = true;
			serverModpack = client.selectedServer.name.Replace(" ", "_");
			if (client.remoteClientModVersion > 0 && ((client.clientModVersion < client.remoteClientModVersion || !doesModpackFileExist(client.clientModVersion)) && !client.offlineMode))
			{
				if (MessageBox.Show("It appears your client's mod package file is outdated or missing.\nWould you like to download the latest?", "Update", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
				{
					statusBlock.Text = "Updating modpack to version " + client.remoteClientModVersion;
					if (doesModpackFileExist(client.clientModVersion))
						File.Delete(client.clientDirectory + "\\modpacks\\zzz" + serverModpack + "-" + client.clientModVersion.ToString() + ".pack");
					downloadFileFromWeb("package/modpack-" + client.remoteClientModVersion, client.clientDirectory + "\\modpacks\\zzz" + serverModpack + "-" + client.remoteClientModVersion + ".pack", client.launcherModRepo);
					return false;
				}
				else
				{
					isUpdateInProgress = false;
					hasUserSkippedUpdate = true;
					MessageBox.Show("You may experience issues without the modpack, some content may be missing or some features may not function.\nYou may now launch the client.");
					return true;
				}
			}
			else
			{
				isUpdateInProgress = false;
			}
			return true;
		}

		public bool doesModpackFileExist(int modVersion)
		{
			if(modVersion > 0)
				return File.Exists(client.clientDirectory + "\\modpacks\\zzz" + serverModpack + "-" + modVersion.ToString() + ".pack");
			else
				return true;
		}

		private void downloadFileFromWeb(String pathToFile, String pathToSave, String host)
		{
			WebClient w = new WebClient();
			w.DownloadFileCompleted += new AsyncCompletedEventHandler(downloadComplete);
			w.DownloadProgressChanged += new DownloadProgressChangedEventHandler(progressUpdate);
			w.DownloadFileAsync(new Uri(host + pathToFile), pathToSave);
		}
		//Download async functions
		private void progressUpdate(object sender, DownloadProgressChangedEventArgs e)
		{
			progressBar.Value = e.ProgressPercentage;
			statusPercentBlock.Text = "(" + e.ProgressPercentage.ToString() + "%)";
		}
		private void downloadComplete(object sender, AsyncCompletedEventArgs e)
		{
			client.writeModVersionData(client.tryGetModpackVersion(client.packDirectory.FullName, serverModpack), clientVersionBlock);
			isUpdateInProgress = false;
			MessageBox.Show("Download of the latest mod package is done!\nYou may now launch the client.");
			statusBlock.Text = "Ready to launch!";
			statusPercentBlock.Text = "";
		}

	}
}
