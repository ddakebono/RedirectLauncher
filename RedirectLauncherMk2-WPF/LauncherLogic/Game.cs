/*
* Copyright 2015 Owen Bennett
* This file is a part of the RedirectLauncher by Owen Bennett.
* All code contained in here is licensed under the MIT license.
* Please fill issue report on https://github.com/ripxfrostbite/RedirectLauncher
*/
using Microsoft.Win32;
using RedirectLauncherMk2_WPF.LauncherLogic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Markup;

namespace RedirectLauncherMk2_WPF
{
	public class Game
	{
		//Client data
		public int clientVersion;
		public int clientModVersion;
		public DirectoryInfo modpackDirectory;
		public DirectoryInfo packDirectory;

		//Server data
		public int remoteClientVersion;
		public int remoteLauncherVersion;
		public int remoteClientModVersion;
		public bool canPatch;
		public String launcherRepo;
		public String launcherModRepo;
		public String loginIp;
		public String langPack;
		public String args;
		public String patchServer;
		public String loginPort;
		public String launcherName;
		public String launcherWebpage;
		public bool offlineMode;
		//Extra data
		public int code = 1622;
		public LauncherSettings settings;
		public String devtools = "Morrighan.exe";
		const String gameBinary = "client.exe";
		public Server selectedServer;


		public Game(LauncherSettings settings)
		{
			this.settings = settings;
			//Get local data
			settings.clientInstallDirectory = locateGameClientDirectory();
			if (File.Exists(settings.clientInstallDirectory + "\\version.dat"))
			{
				clientVersion = BitConverter.ToInt32(File.ReadAllBytes(settings.clientInstallDirectory + "\\version.dat"), 0);
			}
			else
			{
				//This should only be reached should a client version.dat not be found at all
				clientVersion = 0;
				writeVersionData(clientVersion, null);
			}

			modpackDirectory = new DirectoryInfo(settings.clientInstallDirectory + "\\modpacks");
			packDirectory = new DirectoryInfo(settings.clientInstallDirectory + "\\package");
			if (!packDirectory.Exists)
				packDirectory.Create();
			if (!modpackDirectory.Exists)
				modpackDirectory.Create();

			FileInfo[] modpacks = packDirectory.GetFiles("zzz*");
			foreach (var file in modpacks)
				file.MoveTo(modpackDirectory.FullName + "\\" + file.Name);

			clientModVersion = 0;

			handlePatchData(new Dictionary<String, String>());
		}

		public void LaunchGame()
		{
			//Move server modpack into package directory
			FileInfo pack = getCurrentModpack();
			if (pack != null)
			{
				pack.MoveTo(packDirectory.FullName + "\\" + pack.Name);
			}

			Directory.SetCurrentDirectory(settings.clientInstallDirectory);
			String launchArgs = "code:" + code + " ver:" + clientVersion + " logip:" + loginIp + " logport:" + loginPort + " " + args;
			if (selectedServer.usingNXAuth)
				launchArgs += " /P:" + selectedServer.getNxPassport();
			//Launch kanan
			if (File.Exists(settings.kananFolder + "\\kanan.py") && settings.launchKanan)
			{
				ProcessStartInfo kananLaunch = new ProcessStartInfo();
				kananLaunch.FileName = "python.exe";
				kananLaunch.UseShellExecute = false;
				kananLaunch.WorkingDirectory = settings.kananFolder;
				kananLaunch.Arguments = "kanan.py";
				Process.Start(kananLaunch);
			}
			//Launch game binary or dev tool
			if (File.Exists(settings.clientInstallDirectory + "\\" + gameBinary) && Process.GetProcessesByName(gameBinary).Length == 0)
			{
				ProcessStartInfo mabiLaunch = new ProcessStartInfo();
				mabiLaunch.Arguments = launchArgs;
				if (File.Exists(settings.clientInstallDirectory + "\\" + devtools) && settings.launchDevTools)
				{
					mabiLaunch.FileName = settings.clientInstallDirectory + "\\" + devtools;
				}
				else
				{
					mabiLaunch.FileName = settings.clientInstallDirectory + "\\" + gameBinary;
				}
				Process.Start(mabiLaunch);
				System.Environment.Exit(0);
			}
		}

		public String getLocalClientVersionString()
		{
			return clientVersion + "." + clientModVersion;
		}
		public String getRemoteClientVersionString()
		{
			return remoteClientVersion + "." + remoteClientModVersion;
		}

		private String locateGameClientDirectory()
		{
			RegistryKey mabinogiRegistry = Registry.CurrentUser.OpenSubKey(@"Software\Nexon\Mabinogi", true);
			RegistryKey steamRegistry = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam", false);
			String mabiRegDirectory = null;
			String steamCommon = null;
			if (mabinogiRegistry == null)
			{
				mabinogiRegistry = Registry.CurrentUser.CreateSubKey(@"Software\Nexon\Mabinogi");
			}
			mabiRegDirectory = (String)mabinogiRegistry.GetValue("");
			if (steamRegistry != null)
			{
				steamCommon = (String)steamRegistry.GetValue("SteamPath") + "\\steamapps\\common\\Mabinogi";
			}
			String result;
			if (settings.clientInstallDirectory.Length > 0 && File.Exists(settings.clientInstallDirectory + "\\version.dat"))
			{
				//This will be set once the launcher is run for the first time.
				result = settings.clientInstallDirectory;
			}
			else
			{
				if (mabiRegDirectory != null && File.Exists(mabiRegDirectory + "\\version.dat"))
				{
					//If mabi exists in it's default directory
					result = mabiRegDirectory;
				}
				else if (steamCommon != null && File.Exists(steamCommon + "\\version.dat"))
				{
					//If mabi is installed from steam
					result = steamCommon;
				}
				else if (File.Exists(System.Environment.CurrentDirectory + "\\version.dat"))
				{
					//If launcher is in the client directory
					result = System.Environment.CurrentDirectory;
				}
				else
				{
					//User must define a client directory
					FolderBrowserDialog find = new FolderBrowserDialog();
					find.Description = "Select the Mabinogi Client Directory.\nIf one doesn't exist just choose anywhere, the launcher will start a full download.";
					DialogResult selection = find.ShowDialog();
					String directory = find.SelectedPath;
					if (File.Exists(directory + "\\version.dat"))
					{
						//User defined folder has required data
						result = directory;
					}
					else
					{
						//Trigger full client install in standard directory
						Directory.CreateDirectory("C:\\Nexon\\Mabinogi");
						result = "C:\\Nexon\\Mabinogi";
					}
				}
			}
			settings.clientInstallDirectory = result;
			settings.saveToRegistry();
			return result;
		}

		public void writeVersionData(int newVersion, TextBlock clientVersionBlock)
		{
			//This file is updated with the update download
			//File.WriteAllBytes(settings.clientInstallDirectory + "\\version.dat", BitConverter.GetBytes(newVersion));
			clientVersion = BitConverter.ToInt32(File.ReadAllBytes(settings.clientInstallDirectory + "\\version.dat"), 0);
			if (clientVersionBlock != null)
			{
				clientVersion = BitConverter.ToInt32(File.ReadAllBytes(settings.clientInstallDirectory + "\\version.dat"), 0);
				clientVersionBlock.Text = getLocalClientVersionString();
			}
		}

		public void writeModVersionData(int version, TextBlock clientVersionBlock)
		{
			clientModVersion = version;
			clientVersionBlock.Text = getLocalClientVersionString();
		}

		public int tryGetModpackVersion(string path, string server)
		{
			int version = 0;
			DirectoryInfo dir = new DirectoryInfo(path);
			FileInfo[] matchedPacks = dir.GetFiles("zzz" + server + "*");
			Regex pattern = new Regex("zzz" + server + "-(\\d+).*");
			foreach (var file in matchedPacks)
			{
				Match matches = pattern.Match(file.Name);
				if (matches.Groups.Count == 2)
				{
					int regexVersion = int.Parse(matches.Groups[1].Value);
					if (regexVersion > version)
						version = regexVersion;
				}
			}
			return version;
		}

		public FileInfo getCurrentModpack()
		{
			FileInfo[] pack = modpackDirectory.GetFiles("zzz" + selectedServer.name.Replace(' ', '_') + "-" + tryGetModpackVersion(modpackDirectory.FullName, selectedServer.name.Replace(' ', '_')) + ".pack");
			if (pack.Length == 1)
			{
				return pack[0];
			}
			return null;
		}

		public void handlePatchData(Dictionary<String, String> patchdata)
		{
			try
			{
				remoteClientVersion = int.Parse(patchdata["main_version"]);
			}
			catch (KeyNotFoundException e)
			{
				remoteClientVersion = 0;
			}
			try
			{
				remoteLauncherVersion = int.Parse(patchdata["redirectlauncherver"]);
			}
			catch (KeyNotFoundException e)
			{
				remoteLauncherVersion = 0;
			}
			try
			{
				canPatch = (patchdata["patch_accept"] == "0" ? false : true);
			}
			catch (KeyNotFoundException e)
			{
				canPatch = false;
			}
			try
			{
				loginIp = patchdata["login"];
			}
			catch (KeyNotFoundException e)
			{
				loginIp = "127.0.0.1";
			}
			try
			{
				langPack = patchdata["lang"];
			}
			catch (KeyNotFoundException e)
			{
				langPack = "";
			}
			try
			{
				args = patchdata["arg"];
			}
			catch (KeyNotFoundException e)
			{
				args = "setting:\"file://data/features.xml=Regular, USA\"";
			}
			try
			{
				launcherName = patchdata["redirectlaunchername"];
			}
			catch (KeyNotFoundException e)
			{
				launcherName = "Redirect Launcher";
			}
			try
			{
				patchServer = patchdata["main_ftp"];
			}
			catch (KeyNotFoundException e)
			{
				patchServer = "";
			}
			try
			{
				loginPort = patchdata["login_port"];
			}
			catch (KeyNotFoundException e)
			{
				loginPort = "11000";
			}
			try
			{
				launcherRepo = patchdata["redirectlauncherrepo"];
			}
			catch (KeyNotFoundException e)
			{
				launcherRepo = "";
			}
			try
			{
				remoteClientModVersion = int.Parse(patchdata["redirect_mod_version"]);
			}
			catch (KeyNotFoundException e)
			{
				remoteClientModVersion = 0;
			}
			try
			{
				launcherModRepo = patchdata["redirect_mod_repo"];
			}
			catch (KeyNotFoundException e)
			{
				launcherModRepo = "";
			}
			try
			{
				launcherWebpage = patchdata["redirect_launcher_webpage"];
			}
			catch (KeyNotFoundException e)
			{
				if (selectedServer != null && selectedServer.launcherPage != null)
					launcherWebpage = selectedServer.launcherPage;
				else
					launcherWebpage = "about:blank";
			}
		}

		public void loadNewPatchUrl(Server server)
		{
			selectedServer = server;
			clientModVersion = tryGetModpackVersion(modpackDirectory.FullName, selectedServer.name.Replace(' ', '_'));
			Dictionary<String, String> data = selectedServer.patchData(this);
			handlePatchData(data);
		}
	}
}
