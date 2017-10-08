using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RedirectLauncherMk2_WPF.Updaters
{
	class ModUpdateNew
	{
		private Game client;
		private string serverModpack;
		private readonly object syncLock = new object();

		public ModUpdateNew(Game client)
		{
			this.client = client;
		}

		public async Task startModUpdate(IProgress<int> progress, IProgress<String> status)
		{
			await Task.Run(() =>
			{
				serverModpack = client.selectedServer.name.Replace(" ", "_");
				if(doesModpackFileExist(client.clientModVersion))
					File.Delete(client.settings.clientInstallDirectory + "\\modpacks\\zzz" + serverModpack + "-" + client.clientModVersion.ToString() + ".pack");
				WebClient web = new WebClient();
				web.DownloadProgressChanged += (o, e) => {
					progress.Report(e.ProgressPercentage);
				};
				web.DownloadFileCompleted += completedDownload;
				lock (syncLock)
				{
					web.DownloadFileAsync(new Uri(client.launcherModRepo + "package/modpack-" + client.remoteClientModVersion + ".pack"), client.settings.clientInstallDirectory + "\\modpacks\\zzz" + serverModpack + "-" + client.remoteClientModVersion + ".pack", syncLock);
					Monitor.Wait(syncLock);
				}
			});
		}

		public void completedDownload(object sender, AsyncCompletedEventArgs e)
		{
			lock (e.UserState)
			{
				Monitor.Pulse(e.UserState);
			}
		}

		public bool doesModpackFileExist(int modVersion)
		{
			if (modVersion > 0)
				return File.Exists(client.settings.clientInstallDirectory + "\\modpacks\\zzz" + serverModpack + "-" + modVersion.ToString() + ".pack");
			else
				return true;
		}
	}
}
