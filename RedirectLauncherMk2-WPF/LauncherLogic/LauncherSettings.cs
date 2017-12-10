using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedirectLauncherMk2_WPF.LauncherLogic
{
	public class LauncherSettings
	{
		public int selectedServer = 0;
		public String kananFolder = "";
		public bool launchKanan = false;
		public bool launchDevTools = false;
		public String customLoginIP = "";
		private RegistryKey LauncherSubkey;
		public String clientInstallDirectory = "";

		public LauncherSettings()
		{
			loadFromRegistry();
		}

		public void loadFromRegistry()
		{
			LauncherSubkey = Registry.CurrentUser.OpenSubKey(@"Software\RedirectGaming\RedirectLauncher-" + Properties.Settings.Default.AppID, true);
			if (LauncherSubkey == null) 
			{
				LauncherSubkey = Registry.CurrentUser.CreateSubKey(@"Software\RedirectGaming\RedirectLauncher-" + Properties.Settings.Default.AppID);
				saveToRegistry();
			}
			selectedServer = Convert.ToInt16(LauncherSubkey.GetValue("SelectedServer"));
			kananFolder = Convert.ToString(LauncherSubkey.GetValue("KananInstallFolder"));
			launchKanan = Convert.ToBoolean(LauncherSubkey.GetValue("LaunchKanan"));
			launchDevTools = Convert.ToBoolean(LauncherSubkey.GetValue("LaunchDevTools"));
			customLoginIP = Convert.ToString(LauncherSubkey.GetValue("CustomLoginIP"));
			clientInstallDirectory = Convert.ToString(LauncherSubkey.GetValue("ClientInstallDirectory"));
		}

		public void saveToRegistry()
		{
			LauncherSubkey.SetValue("SelectedServer", selectedServer, RegistryValueKind.DWord);
			LauncherSubkey.SetValue("KananInstallFolder", kananFolder, RegistryValueKind.String);
			LauncherSubkey.SetValue("LaunchKanan", launchKanan, RegistryValueKind.DWord);
			LauncherSubkey.SetValue("LaunchDevTools", launchDevTools, RegistryValueKind.DWord);
			LauncherSubkey.SetValue("CustomLoginIP", customLoginIP, RegistryValueKind.String);
			LauncherSubkey.SetValue("ClientInstallDirectory", clientInstallDirectory, RegistryValueKind.String);
		}
	}
}
