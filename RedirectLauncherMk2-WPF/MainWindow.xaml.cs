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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RedirectLauncherMk2_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private Mabinogi client = new Mabinogi("http://aurares.potato.moe/patchdata.txt");
        private ClientUpdater clientUpdater;
        private ModUpdater modUpdater;
        private SelfUpdater updater;
        private bool skipModpackUpdate;

        public MainWindow()
        {
            updater = new SelfUpdater(client.launcherRepo, Properties.Settings.Default.Version, client.remoteLauncherVersion);
            clientUpdater = new ClientUpdater(client);
            modUpdater = new ModUpdater(client);
            InitializeComponent();
        }

        private void windowIsReady(object sender, EventArgs e)
        {
            ClientVersionBlock.Text = client.getLocalClientVersionString();
            LauncherVersionBlock.Text = Properties.Settings.Default.Version.ToString();
            RemoteClientVersionBlock.Text = client.getRemoteClientVersionString();
            RemoteLauncherVersionBlock.Text = client.remoteLauncherVersion.ToString();
            TitleBlock.Text = client.launcherName;
            updater.checkLauncherUpdates(ProgressBar);
        }

        private void LaunchGame(object sender, RoutedEventArgs e)
        {
            modUpdater.startModUpdate(ProgressBar, ClientVersionBlock);
            clientUpdater.checkClientUpdate(ProgressBar, ClientVersionBlock);
            if (client.clientVersion == client.remoteClientVersion && ((client.clientModVersion == client.remoteClientModVersion && modUpdater.doesModpackFileExist() && !modUpdater.isUpdateInProgress) || (modUpdater.hasUserSkippedUpdate)) && !clientUpdater.isUpdateInProgress)
            {
                client.LaunchGame();
            }
        }
    }
}
