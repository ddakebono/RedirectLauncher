using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace RedirectLauncherMk2_WPF
{
    class Mabinogi
    {
        //Client data
        public int clientVersion;
        public String clientDirectory;
        //Server data
        public int remoteClientVersion;
        public int remoteLauncherVersion;
        public bool canPatch;
        public bool isRedirectReady;
        public String launcherRepo;
        public String loginIp;
        public String langPack;
        public String args;
        public String patchServer;
        public String loginPort;
        public String launcherName;
        //Extra data
        public int code = 1622;
        public String crackShield = "HSLaunch.exe";


        public Mabinogi(String patchURL)
        {
            //Get local data
            RegistryKey mabi = Registry.CurrentUser.OpenSubKey(@"Software\Nexon\Mabinogi", false);
            clientDirectory = (String)mabi.GetValue("");
            mabi.Close();
            if (File.Exists(clientDirectory + "\\version.dat"))
            {
                clientVersion = BitConverter.ToInt32(File.ReadAllBytes(clientDirectory + "\\version.dat"), 0);
            }

            //Get remote patch info
            Dictionary<String, String> patchdata = patchData(patchURL);
            //Loads data retrieved from the patch data url into the class variables
            handlePatchData(patchdata);
        }

        public void LaunchGame()
        {
            Directory.SetCurrentDirectory(clientDirectory);
            String launchArgs = "code:" + code + " ver:" + clientVersion + " logip:" + loginIp + " logport:" + loginPort + " " + args;
            if(File.Exists(clientDirectory + "\\" + crackShield) && Process.GetProcessesByName(crackShield).Length == 0){
                ProcessStartInfo crackShieldStart = new ProcessStartInfo();
                crackShieldStart.FileName = clientDirectory + "\\" + crackShield;
                Process.Start(crackShieldStart);
            }
            if (File.Exists(clientDirectory + "\\client.exe") && Process.GetProcessesByName("client.exe").Length == 0) 
            {
                ProcessStartInfo mabiLaunch = new ProcessStartInfo();
                mabiLaunch.Arguments = launchArgs;
                mabiLaunch.FileName = clientDirectory + "\\client.exe";
                Process.Start(mabiLaunch);
                System.Environment.Exit(0);
            }
        }

        public void writeVersionData(int newVersion, TextBlock clientVersionBlock)
        {
            File.WriteAllBytes(clientDirectory + "\\version.dat", BitConverter.GetBytes(newVersion));
            clientVersion = BitConverter.ToInt32(File.ReadAllBytes(clientDirectory + "\\version.dat"), 0);
            clientVersionBlock.Text = clientVersion.ToString();
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
                launcherName = "Redirect Gaming Mabinogi Launcher";
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
                isRedirectReady = (patchdata["redirectenabled"] == "1" ? true : false);
            }
            catch (KeyNotFoundException e)
            {
                isRedirectReady = false;
            }
            try
            {
                launcherRepo = patchdata["redirectlauncherrepo"];
            }
            catch (KeyNotFoundException e)
            {
                launcherRepo = "";
            }
        }

        private Dictionary<String, String> patchData(String patchUrl)
        {
            HttpWebRequest initConnection = (HttpWebRequest)WebRequest.Create(patchUrl);
            HttpWebResponse recievedData = (HttpWebResponse)initConnection.GetResponse();
            Stream dataStream = recievedData.GetResponseStream();
            Encoding enc = Encoding.UTF8;
            StreamReader r = new StreamReader(dataStream, enc);
            Dictionary<String, String> result = new Dictionary<string,string>();
            String tempLine;
            while ((tempLine = r.ReadLine()) != null)
            {
                if (tempLine.Trim().Length > 0 && !tempLine[0].Equals("#"))
                {
                    String[] tempSplit = tempLine.Split(new char[] { '=' }, 2);
                    if (tempSplit.Length == 2)
                    {
                        result.Add(tempSplit[0], tempSplit[1]);
                    }
                }
            }
            recievedData.Close();
            dataStream.Close();
            return result;
        }
    }
}
