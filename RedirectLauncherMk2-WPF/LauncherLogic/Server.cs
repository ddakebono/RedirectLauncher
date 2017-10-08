﻿/*
* Copyright 2015 Owen Bennett
* This file is a part of the RedirectLauncher by Owen Bennett.
* All code contained in here is licensed under the MIT license.
* Please fill issue report on https://github.com/ripxfrostbite/RedirectLauncher
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Net;
using System.IO;
using System.Security.Cryptography;
using System.Management;
using Newtonsoft.Json;
using System.Net.Http;
using RedirectLauncherMk2_WPF.LauncherLogic;
using NxApiCommon;

namespace RedirectLauncherMk2_WPF
{
	public class Server
	{
		public string name { get; set; }
		public string patchdata { get; set; }
		public string launcherPage { get; set; }
		public int resWidth { get; set; }
		public int resHeight { get; set; }
		public string username { get; set; }
		public string password { get; set; }
		public Boolean usingNXAuth { get; set; }
		private string b64ApiToken;
		private NxApiClient restAPI;
		private readonly string appID = "10200";

		public Dictionary<String, String> patchdataOverride { get; set; }

		public Server(string name, string patchdata, string launcherPage = null, int resWidth = 0, int resHeight = 0, string username = null, string password = null, Boolean usingNXAuth = false, Dictionary<String, String> patchdataOverride = null)
		{
			this.name = name;
			this.patchdata = patchdata;
			this.launcherPage = launcherPage;
			this.resHeight = resHeight;
			this.resWidth = resWidth;
			this.patchdataOverride = patchdataOverride;
			this.username = username;
			this.password = password;
			this.usingNXAuth = usingNXAuth;
		}

		public Server()
		{
			
		}

		public int getNxVersion()
		{
			if (restAPI == null)
				restAPI = new NxApiClient(username, password, appID);

			return restAPI.getVersion();
		}

		public string getNxPassport()
		{
			if (restAPI == null)
				restAPI = new NxApiClient(username, password, appID);

			return restAPI.getNxPassport();
		}

		public Dictionary<String, String> patchData(Game client)
		{
			Dictionary<String, String> result = new Dictionary<string, string>();
			try
			{
				HttpWebRequest initConnection = (HttpWebRequest)WebRequest.Create(patchdata);
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
			catch (Exception e)
			{
				if (e is WebException || e is UriFormatException)
				{
					System.Windows.MessageBox.Show("The launcher cannot connect to the server containing the patch info");
					client.offlineMode = true;
				}
				else
				{
					throw;
				}
			}
			//Load in overridden values
			if (patchdataOverride != null)
			{
				foreach (var keypair in patchdataOverride)
				{
					try
					{
						result[keypair.Key] = keypair.Value;
					}
					catch (KeyNotFoundException e)
					{
						result.Add(keypair.Key, keypair.Value);
					}
				}
			}
			return result;
		}

	}
}
