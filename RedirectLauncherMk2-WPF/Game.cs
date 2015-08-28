/*
* Copyright 2015 Owen Bennett
* This file is a part of the Redirect-Launcher-MK2 by Owen Bennett.
* All code contained in here is licensed under the MIT license.
* Please fill issue report on http://git.potato.moe/potato-san/Redirect-Launcher-mk2
*/

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
using System.Windows.Forms;
using System.Windows.Markup;

namespace RedirectLauncherMk2_WPF
{
    class Game
    {
        //Client data
        public int clientVersion;
        public int clientModVersion;
        public String clientDirectory;
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
        public String crackShield = "HSLaunch.exe";
        public String devtools = "Morrighan.exe";
        const String gameBinary = "client.exe";


        public Game(String patchURL)
        {
            //Get local data
            clientDirectory = locateGameClientDirectory();
            if (File.Exists(clientDirectory + "\\version.dat"))
            {
                clientVersion = BitConverter.ToInt32(File.ReadAllBytes(clientDirectory + "\\version.dat"), 0);
            }
            else
            {
                //This should only be reached should a client version.dat not be found at all
                clientVersion = 0;
                writeVersionData(clientVersion, null);
            }
            if (File.Exists(clientDirectory + "\\modVersion.dat"))
            {
                clientModVersion = BitConverter.ToInt32(File.ReadAllBytes(clientDirectory + "\\modVersion.dat"), 0);
            }
            else
            {
                clientModVersion = 0;
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
            if (File.Exists(clientDirectory + "\\" + gameBinary) && Process.GetProcessesByName(gameBinary).Length == 0) 
            {
                ProcessStartInfo mabiLaunch = new ProcessStartInfo();
                mabiLaunch.Arguments = launchArgs;
                if (File.Exists(clientDirectory + "\\" + devtools))
                {
                    mabiLaunch.FileName = clientDirectory + "\\" + devtools;
                }
                else
                {
                    mabiLaunch.FileName = clientDirectory + "\\" + gameBinary;
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
            String redirectRegKey = null;
            String mabiRegDirectory = null;
            String steamCommon = null;
            if (mabinogiRegistry == null)
            {
                mabinogiRegistry = Registry.CurrentUser.CreateSubKey(@"Software\Nexon\Mabinogi");
            }
            redirectRegKey = (String)mabinogiRegistry.GetValue("RDClientRoot");
            mabiRegDirectory = (String)mabinogiRegistry.GetValue("");
            if (steamRegistry != null)
            {
                steamCommon = (String)steamRegistry.GetValue("SteamPath") + "\\steamapps\\common\\Mabinogi";
            }
            String result;
            if (redirectRegKey != null && File.Exists(redirectRegKey + "\\version.dat"))
            {
                //This will be set once the launcher is run for the first time.
                result = redirectRegKey;
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
            mabinogiRegistry.SetValue("RDClientRoot", result, RegistryValueKind.String);
            return result;
        }

        public void writeVersionData(int newVersion, TextBlock clientVersionBlock)
        {
            File.WriteAllBytes(clientDirectory + "\\version.dat", BitConverter.GetBytes(newVersion));
            if (clientVersionBlock != null)
            {
                clientVersion = BitConverter.ToInt32(File.ReadAllBytes(clientDirectory + "\\version.dat"), 0);
                clientVersionBlock.Text = getLocalClientVersionString();
            }
        }
        public void writeModVersionData(int newVersion, TextBlock clientVersionBlock)
        {
            File.WriteAllBytes(clientDirectory + "\\modVersion.dat", BitConverter.GetBytes(newVersion));
            clientModVersion = BitConverter.ToInt32(File.ReadAllBytes(clientDirectory + "\\modVersion.dat"), 0);
            clientVersionBlock.Text = getLocalClientVersionString();
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
                launcherName = "Redirect Gaming Launcher";
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
                launcherWebpage = "about:blank";
            }
        }

        private Dictionary<String, String> patchData(String patchUrl)
        
        {
            Dictionary<String, String> result = new Dictionary<string, string>();
            try
            {
                HttpWebRequest initConnection = (HttpWebRequest)WebRequest.Create(patchUrl);
                HttpWebResponse recievedData = (HttpWebResponse)initConnection.GetResponse();
                Stream dataStream = recievedData.GetResponseStream();
                Encoding enc = Encoding.UTF8;
                StreamReader r = new StreamReader(dataStream, enc);
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
            }
            catch (WebException e)
            {
                System.Windows.MessageBox.Show("The launcher cannot connect to the server containing the patch info");
                offlineMode = true;
            }
            //Overrides from the launchers arguments
            String[] overrideArguments = Environment.GetCommandLineArgs();
            for (int i = 0; i < overrideArguments.Length; i++)
            {
                String[] tempSplit = overrideArguments[i].Trim().Split(new char[] {'='}, 2);
                if (tempSplit.Length == 2)
                {
                    try
                    {
                        result[tempSplit[0]] = tempSplit[1];
                    }
                    catch (KeyNotFoundException e)
                    {
                        result.Add(tempSplit[0], tempSplit[1]);
                    }
                }
            }
            return result;
        }
    }
}
