using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace RedirectLauncherMk2_WPF
{
	[Serializable]
	class Server
	{
		public string Name { get; private set; }
		public string Patchdata { get; private set; }
		public string launcherPage { get; private set; }

		public Server(string Name, string Patchdata, string launcherPage)
		{
			this.Name = Name;
			this.Patchdata = Patchdata;
			this.launcherPage = launcherPage;
		}
	}
	public class Servers : DatabaseJsonIndexed<string, Server>
	{

	}
}
