/*
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

namespace RedirectLauncherMk2_WPF
{
	public class Server
	{
		public string name { get; set; }
		public string patchdata { get; set; }
		public string launcherPage { get; set; }
		public int resWidth { get; set; }
		public int resHeight { get; set; }

		public Server(string name, string patchdata, string launcherPage = null, int resWidth = 0, int resHeight = 0)
		{
			this.name = name;
			this.patchdata = patchdata;
			this.launcherPage = launcherPage;
			this.resHeight = resHeight;
			this.resWidth = resWidth;
		}
	}
}
