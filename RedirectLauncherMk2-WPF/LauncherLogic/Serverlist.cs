using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedirectLauncherMk2_WPF
{
	public class Serverlist
	{
		public List<Server> servers;

		public Serverlist()
		{
			loadFromFile();
		}

		public void loadFromFile()
		{
			servers = new List<Server>();
			StreamReader jsonFile = File.OpenText("Servers.json");
			JsonTextReader reader = new JsonTextReader(jsonFile);
			JsonSerializer json = new JsonSerializer();
			var serversFile = json.Deserialize<Server[]>(reader);
			foreach (var server in serversFile)
			{
				servers.Add(server);
			}
		}

		public void saveToFile()
		{
			JsonSerializer json = new JsonSerializer();
			StreamWriter sw = new StreamWriter(File.Create("Servers.json"));
			JsonWriter jw = new JsonTextWriter(sw);

			jw.WriteStartArray();
			foreach (var server in servers)
			{
				sw.WriteLine();
				json.Serialize(jw, server);
			}
			sw.WriteLine();
			jw.WriteEndArray();
			jw.Close();
			sw.Close();
		}
	}
}
