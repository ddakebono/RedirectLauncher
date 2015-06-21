using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.FtpClient;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RedirectLauncherMk2_WPF
{
    class ClientUpdater
    {
        private Mabinogi client;
        private DirectoryInfo updateDirectory;
        private DirectoryInfo updateExtractDirectory;

        public ClientUpdater(Mabinogi client)
        {
            this.client = client;
            this.updateDirectory = new DirectoryInfo(System.Environment.CurrentDirectory + "\\rdlauncherMabiUpdaterTemp");
            this.updateExtractDirectory = new DirectoryInfo(updateDirectory.FullName + "\\extracted");
        }

        public void checkClientUpdate()
        {
            if (client.clientVersion < client.remoteClientVersion)
            {
                if (MessageBox.Show("It appears your client is out of date!\nWould you like to update to the latest client version?", "Update", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    //Start update
                    prepareUpdateDirectory();
                    startUpdate();
                }
                else
                {
                    MessageBox.Show("The server shows that a newer client is needed, you will probably not be able to play until you update.");
                }
            }
        }

        private void startUpdate()
        {
            FtpClient downloader = new FtpClient();
            if (client.patchServer.Equals("mabipatch.nexon.net/game/"))
            {
                FtpClient.Connect(new Uri("mabipatch.nexon.net"));
            }
            else
            {
                FtpClient.Connect(new Uri(client.patchServer));
            }
            if (downloadFileFromFtp(client.remoteClientVersion + "/" + client.clientVersion + "_to_" + client.remoteClientVersion + ".txt", updateDirectory.FullName + "update.txt", downloader))
            {
                //Start normal update process
            }
            else
            {
                //Begin full client download
            }

        }

        private bool downloadFileFromFtp(String pathToFile, String pathToSave, FtpClient downloader){
            bool downloadComplete = true;
            Stream f = downloader.OpenRead(pathToFile);
            FileInfo save = new FileInfo(pathToSave);
            save.Create();
            Stream fo = save.OpenWrite();
            byte[] buffer = new byte[8 * 1024];
            int count;
            while ((count = f.Read(buffer, 0, buffer.Length)) > 0)
            {
                fo.Write(buffer, 0, count);
            }
            fo.Close();
            f.Close();
            return downloadComplete;
        }

        private void prepareUpdateDirectory()
        {
            if (updateDirectory.Exists == true)
            {
                updateDirectory.Delete(true);
                updateDirectory.Create();
                updateExtractDirectory.Create();
            }
            else
            {
                updateDirectory.Create();
                updateExtractDirectory.Create();
            }
        }
    }
}
