/*
* Copyright 2015 Owen Bennett
* This file is a part of the RedirectLauncher by Owen Bennett.
* All code contained in here is licensed under the MIT license.
* Please fill issue report on https://github.com/ripxfrostbite/RedirectLauncher
*/
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using RedirectLauncherMk2_WPF.LauncherLogic;
using RedirectLauncherMk2_WPF.Updater;
using RedirectLauncherMk2_WPF.Updaters;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Navigation;

namespace RedirectLauncherMk2_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public LauncherSettings settings;
        public Game client;
        private ClientUpdater clientUpdater;
        private ModUpdateNew modUpdater;
        private SelfUpdater updater;
        public Serverlist serverList;
        public SynchronizationContext synchronization;
        private bool pageHasLoaded = false;
        private bool launching = false;
        public bool clientDirChanged;
        public bool selectedServerChanged;

        public MainWindow()
        {
            settings = new LauncherSettings();
            client = new Game(settings);
            serverList = new Serverlist();
            InitializeComponent();
            synchronization = SynchronizationContext.Current;
        }

        private void windowIsReady(object sender, EventArgs e)
        {
            client.loadNewPatchUrl((settings.selectedServer < serverList.servers.Count) ? serverList.servers[settings.selectedServer] : serverList.servers.First());
            reloadElements();
            updater = new SelfUpdater(client.launcherRepo, Properties.Settings.Default.Version, client.remoteLauncherVersion);
            updater.checkLauncherUpdates(ProgressBar, StatusBlock);
            clientUpdater = new ClientUpdater(client, this);
            modUpdater = new ModUpdateNew(client);
            reloadElements();
            StatusBlock.Text = "Ready to Launch!";

        }

        public void reloadElements()
        {
            ClientVersionBlock.Text = client.getLocalClientVersionString();
            LauncherVersionBlock.Text = Properties.Settings.Default.Version.ToString();
            RemoteClientVersionBlock.Text = client.getRemoteClientVersionString();
            RemoteLauncherVersionBlock.Text = client.remoteLauncherVersion.ToString();
            pageHasLoaded = false;
            if (client.selectedServer.resWidth > 0)
                WebBlock.Width = client.selectedServer.resWidth;
            else
                WebBlock.Width = Double.NaN;
            if (client.selectedServer.resHeight > 0)
                WebBlock.Height = client.selectedServer.resHeight;
            else
                WebBlock.Height = Double.NaN;
            this.Title = client.launcherName;
            try
            {
                if (!String.IsNullOrEmpty(client.launcherWebpage))
                {
                    FallbackImage.Visibility = Visibility.Hidden;
                    WebBlock.Visibility = Visibility.Visible;
                    WebBlock.Source = new Uri(client.launcherWebpage);
                }
                else
                {
                    //Fallback background
                    FallbackImage.Visibility = Visibility.Visible;
                    WebBlock.Visibility = Visibility.Hidden;
                }
            }
            catch (UriFormatException e)
            {
                System.Windows.Forms.MessageBox.Show("The URL given for Launcher Webpage was invalid!");
                WebBlock.Source = new Uri("about:blank");
            }
        }

        private async void LaunchGame(object sender, RoutedEventArgs e)
        {
            if (!launching)
            {
                launching = true;
                settings.saveToRegistry();
                if (client.clientVersion < client.remoteClientVersion)
                {
                    if (await clientUpdater.loadManifestForVersion(client.remoteClientVersion, client.selectedServer.patchObjectsHost, client.mainVersionHash, client.selectedServer.getManifestURL()))
                    {
                        StatusBlock.Text = "Processing manifest and parsing install files...";
                        await clientUpdater.getInstallDiff(new Progress<int>(p =>
                        {
                            ProgressBar.Value = p;
                            StatusPercentBlock.Text = "(" + p.ToString() + "%)";
                        }));

                        MessageDialogResult result = await displayAlertDialog("Update Required", clientUpdater.FilesNeedingUpdate.Count + " Files need to be downloaded to upgrade to version " + client.remoteClientVersion, MessageDialogStyle.AffirmativeAndNegative);
                        if (result.Equals(MessageDialogResult.Affirmative))
                        {
                            StatusBlock.Text = "Downloading updated files";
                            if (await clientUpdater.startUpdate(new Progress<int>(p =>
                                 {
                                     ProgressBar.Value = p;
                                     StatusPercentBlock.Text = "(" + p.ToString() + "%)";
                                 }), new Progress<String>(p =>
                                 {
                                     StatusBlock.Text = p;
                                 })))
                            {
                                client.writeVersionData(client.remoteClientVersion, ClientVersionBlock, false);
                                await displayAlertDialog("Update Complete!", "The update has completed successfully, you may now launch the client!");
                                StatusBlock.Text = "Update Completed!";
                            }
                        }
                    }
                    else
                    {
                        await displayAlertDialog("Update Failed!", clientUpdater.lastError);
                        StatusBlock.Text = "Update failed!";
                    }
                    launching = false;
                }
                else
                {
                    launching = true;
                    if (client.clientModVersion >= client.remoteClientModVersion)
                    {
                        client.LaunchGame();
                    }
                    else
                    {
                        MessageDialogResult result = await displayAlertDialog("Modpack Outdated!", "It seems the custom modpack is outdated, would you like to download the latest one?", MessageDialogStyle.AffirmativeAndNegative);
                        if (result.Equals(MessageDialogResult.Affirmative))
                        {
                            StatusBlock.Text = "Downloading modpack version " + client.remoteClientModVersion;
                            await modUpdater.startModUpdate(new Progress<int>(p =>
                            {
                                ProgressBar.Value = p;
                                StatusPercentBlock.Text = "(" + p.ToString() + "%)";
                            }), new Progress<String>(p =>
                            {
                                StatusBlock.Text = p;
                            }));

                            if ((await displayAlertDialog("Modpack Downloaded!", "The modpack has been updated successfully, press OK to launch the client.", MessageDialogStyle.AffirmativeAndNegative)).Equals(MessageDialogResult.Affirmative))
                                client.LaunchGame();
                            StatusBlock.Text = "Modpack downloaded!";
                            launching = false;
                        }
                        else
                        {
                            //Skipped modpack update
                            client.LaunchGame();
                        }
                    }
                }
            }
        }

        private void handleLinks(object sender, NavigatingCancelEventArgs e)
        {
            if (!pageHasLoaded || e.Uri.Equals(client.selectedServer.launcherPage))
            {
                pageHasLoaded = true;
                return;
            }
            e.Cancel = true;
            Process.Start(e.Uri.ToString());
        }

        public async Task<MessageDialogResult> displayAlertDialog(string title, string message, MessageDialogStyle style = MessageDialogStyle.Affirmative)
        {
            WebBlock.Visibility = Visibility.Hidden;
            MessageDialogResult result = await this.ShowMessageAsync(title, message, style);
            WebBlock.Visibility = Visibility.Visible;
            return result;
        }

        private void OpenAboutWindow(object sender, RoutedEventArgs e)
        {

            About about = new About();
            serverList.saveToFile();
            about.ShowDialog();
        }

        private void OpenOptionsWindow(object sender, RoutedEventArgs e)
        {
            WebBlock.Visibility = Visibility.Hidden;
            OptionsFlyout.IsOpen = true;
            ServerList.ItemsSource = serverList.servers;
            ServerList.DisplayMemberPath = "name";
            ServerList.SelectedIndex = serverList.servers.FindIndex(x => x.name.Equals(client.selectedServer.name));
            Kanan.IsChecked = client.settings.launchKanan;
            Proxy.IsChecked = client.settings.launchDevTools;
            FolderSelect.Text = client.settings.kananFolder;
            ClientDir.Text = client.settings.clientInstallDirectory;
            FolderSelect.ToolTip = client.settings.kananFolder;
            ClientDir.ToolTip = client.settings.clientInstallDirectory;
            clientVersion.Value = client.clientVersion;
            clientDirChanged = false;
            selectedServerChanged = false;
        }

        /*
		 *	Options Flyout Methods
		 *  These methods are only used by the options flyout
		 * 
		 * 
		 */

        private void ApplyClick(object sender, RoutedEventArgs e)
        {
            client.settings.clientInstallDirectory = ClientDir.Text;
            client.settings.selectedServer = ServerList.SelectedIndex;
            client.settings.launchKanan = (bool)Kanan.IsChecked;
            client.settings.launchDevTools = (bool)Proxy.IsChecked;
            client.settings.kananFolder = FolderSelect.Text;
            client.settings.saveToRegistry();
            OptionsFlyout.IsOpen = false;
            if (clientDirChanged)
            {
                client = new Game(settings);
            }
            if (selectedServerChanged || clientDirChanged)
            {
                client.loadNewPatchUrl(serverList.servers[settings.selectedServer]);
            }
            reloadElements();
        }

        private void OpenFolderBrowser(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog find = new FolderBrowserDialog();
            find.Description = "Please select the folder that contains the kanan loader.exe file.";
            DialogResult selection = find.ShowDialog();
            String directory = find.SelectedPath;
            if (File.Exists(directory + "\\loader.exe"))
            {
                //User defined folder has required data
                FolderSelect.Text = directory;
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("The selected folder doesn't contain the loader.exe, please choose another one.");
            }

        }

        private async void VerifyClick(object sender, RoutedEventArgs e)
        {
            StatusBlock.Text = "Downloading manifest for version " + clientVersion;
            await clientUpdater.loadManifestForVersion(client.clientVersion, client.selectedServer.patchObjectsHost, client.mainVersionHash, client.selectedServer.getManifestURL());
            StatusBlock.Text = "Verifying client installation data";
            await clientUpdater.getInstallDiff(new Progress<int>(p =>
            {
                ProgressBar.Value = p;
                StatusPercentBlock.Text = "(" + p.ToString() + "%)";
            }));
            if (clientUpdater.FilesNeedingUpdate.Count > 0)
            {
                MessageDialogResult result = await displayAlertDialog("Files failed validation", clientUpdater.FilesNeedingUpdate.Count + " Files failed validation and need to be redownloaded.", MessageDialogStyle.AffirmativeAndNegative);
                if (result.Equals(MessageDialogResult.Affirmative))
                {
                    StatusBlock.Text = "Downloading files";
                    if (await clientUpdater.startUpdate(new Progress<int>(p =>
                    {
                        ProgressBar.Value = p;
                        StatusPercentBlock.Text = "(" + p.ToString() + "%)";
                    }), new Progress<String>(p =>
                    {
                        StatusBlock.Text = p;
                    })))
                    {
                        client.writeVersionData(client.remoteClientVersion, ClientVersionBlock, false);
                        await displayAlertDialog("Verify Complete!", "The modified files have been redownloaded, you may now launch the client!");
                        StatusBlock.Text = "Verify Completed!";
                    }
                }
            }
            else
            {
                StatusBlock.Text = "Verify completed, no files failed validation!";
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

        private void OpenManageServer(object sender, RoutedEventArgs e)
        {
            ServerManager manager = new ServerManager(serverList.servers[ServerList.SelectedIndex]);
            manager.ShowDialog();
            selectedServerChanged = true;
            serverList.saveToFile();
        }

        private void OpenAddServer(object sender, RoutedEventArgs e)
        {
            ServerManager manager = new ServerManager();
            manager.ShowDialog();
            if (manager.server != null)
            {
                serverList.servers.Add(manager.server);
                ServerList.ItemsSource = serverList.servers;
                ServerList.SelectedIndex = serverList.servers.IndexOf(manager.server);
            }
            serverList.saveToFile();
        }

        private void ApplyVersionChange(object sender, RoutedEventArgs e)
        {
            if (System.Windows.MessageBox.Show("You're about to change your version.dat to a different number, this will make the launcher update to the latest version again. Are you sure?", "Change Version", MessageBoxButton.YesNo, MessageBoxImage.Asterisk) == MessageBoxResult.Yes)
            {
                if (clientVersion.Value.HasValue)
                {
                    client.writeVersionData(clientVersion.Value.Value, null, true);
                    selectedServerChanged = true;
                }
            }
        }

        private void OptionsOpenChanged(object sender, RoutedEventArgs e)
        {
            if (!OptionsFlyout.IsOpen && !FallbackImage.Visibility.Equals(Visibility.Visible))
                WebBlock.Visibility = Visibility.Visible;
        }
    }
}
