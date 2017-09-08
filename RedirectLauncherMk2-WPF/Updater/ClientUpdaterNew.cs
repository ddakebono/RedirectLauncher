using MahApps.Metro.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RedirectLauncherMk2_WPF.Updater
{
	class ClientUpdaterNew
	{
		List<ManifestFile> Files = new List<ManifestFile>();
		Queue<ManifestFile> FilesNeedingUpdate = new Queue<ManifestFile>();
		private int TargetVersion = 0;
		private string NexonPatchDomain = "https://download2.nexon.net/Game/nxl/games/10200/"; //We fetch all the manifest data from here
		private Game Client;
		private DirectoryInfo updateDirectory;
		private DirectoryInfo updateExtractDirectory;
		private MainWindow LauncherWindow;

		public ClientUpdaterNew(Game Client, MainWindow LauncherWindow)
		{
			this.Client = Client;
			this.LauncherWindow = LauncherWindow;
			updateDirectory = new DirectoryInfo(System.Environment.CurrentDirectory + "\\rdlauncherMabiUpdaterTemp");
			updateExtractDirectory = new DirectoryInfo(updateDirectory.FullName + "\\extracted");
		}

		private void initDirectories()
		{
			if (!updateDirectory.Exists)
				updateDirectory.Create();
			if (!updateExtractDirectory.Exists)
				updateExtractDirectory.Create();
		}

		public bool loadManifestForVersion(int Version)
		{
			Files.Clear();
			FilesNeedingUpdate.Clear();

			using (var dl = new WebClient())
			{
				try
				{
					dl.DownloadFile(NexonPatchDomain + "10200." + Version + "R.manifest.hash", updateDirectory.FullName + "manifest.hash");
					dl.DownloadFile(NexonPatchDomain + File.ReadAllText(updateDirectory.FullName + "manifest.hash"), updateDirectory.FullName + "update.manifest");
				}
				catch (WebException e)
				{
					if (((HttpWebResponse)e.Response).StatusCode.Equals(HttpStatusCode.NotFound)){
						LauncherWindow.displayAlertDialog("Update Failed!", "It seems that we couldn't get the manifest for version " + Version + " please try later...");
						LauncherWindow.StatusBlock.Text = "Update Failed";
						return false;
					}
				}
			}

			//Got the manifest, process it.
			MemoryStream ms = new MemoryStream(File.ReadAllBytes(updateDirectory.FullName + "update.manifest"));
			ms.ReadByte();
			ms.ReadByte();
			DeflateStream df = new DeflateStream(ms, CompressionMode.Decompress);
			String manifestJson = new StreamReader(df, Encoding.UTF8).ReadToEnd();

			dynamic json = JsonConvert.DeserializeObject<dynamic>(manifestJson);
			foreach (JProperty prop in ((JObject)json.files).Children())
			{
				Files.Add(new ManifestFile(prop.Name, prop.Value["fsize"].ToObject<long>(), prop.Value["mtime"].ToObject<long>(), prop.Value["objects"].ToObject<List<String>>(), prop.Value["objects_fsize"].ToObject<List<String>>()));
			}

			if (Files.Count > 0)
			{
				Console.WriteLine("Manifest loaded with " + Files.Count + " files");
				return true;
			}
			else
			{
				LauncherWindow.displayAlertDialog("Update Failed!", "Looks like something was weird with the manifest for version " + Version + ", it returned no files! Please try again later...");
				return false;
			}
		}

		public void getInstallDiff()
		{
			int currentManifestObject = 0;
			//Check if file is valid
			foreach(ManifestFile file in Files)
			{
				currentManifestObject++;
				LauncherWindow.StatusBlock.Text = "Checking Files: " + currentManifestObject + " of " + Files.Count;
				string filePath = LauncherWindow.settings.clientInstallDirectory + "\\" + file.name;
				if (file.fileParts.Count>0 && file.fileParts[0].Equals("__DIR__"))
					continue;
				FileInfo fileInfo = new FileInfo(filePath);
				if (fileInfo.Exists)
				{
					if (file.fileParts.Count == 1)
					{
						using (FileStream stream = fileInfo.OpenRead())
						{
							//Valid hash
							SHA1Managed sha = new SHA1Managed();
							byte[] hash = sha.ComputeHash(stream);
							string computedHash = BitConverter.ToString(hash).Replace("-", String.Empty).ToLower();
							if (file.fileParts[0].Equals(computedHash))
								continue;
						}
					}
					else
					{
						//Not modified
						if (fileInfo.Length == file.fileSize && fileInfo.LastWriteTimeUtc.Equals(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(file.buildDate)))
							continue;
					}
				}
				FilesNeedingUpdate.Enqueue(file);
			}

			Console.WriteLine("Total files needing download " + FilesNeedingUpdate.Count);
		}
	}

	class ManifestFile
	{
		public String name;
		public long fileSize;
		public long buildDate; //I think?
		public List<String> fileParts;
		public List<String> filePartsSizes;

		public ManifestFile(String name, long fileSize, long buildDate, List<String> fileParts, List<String> filePartsSizes)
		{
			byte[] data = Convert.FromBase64String(name);
			this.name = Encoding.Unicode.GetString(data).Substring(1);
			this.fileSize = fileSize;
			this.buildDate = buildDate;
			this.fileParts = fileParts;
			this.filePartsSizes = filePartsSizes;
		}
	}
}
