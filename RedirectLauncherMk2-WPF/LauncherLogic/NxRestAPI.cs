using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RedirectLauncherMk2_WPF.LauncherLogic
{
	class NxRestAPI
	{
		private string apiToken;
		private double accessTokenExpires = 0;
		public string passportToken;
		private Server server;
		private RestClient client;
		private CookieContainer cookies;

		public NxRestAPI(Server server)
		{
			this.server = server;
			client = new RestClient("https://api.nexon.io");
			client.UserAgent = "NexonLauncher.nxl-17.05.05-479-4a3d51e";
			cookies = new CookieContainer();
			client.CookieContainer = cookies;
		}

		/*public String getNxPassport()
		{
			if (b64ApiToken == null)
				return null;

			//Create second web request to get passport
			HttpWebRequest reqPassport = (HttpWebRequest)WebRequest.Create("https://api.nexon.io/users/me/passport");
			reqPassport.UserAgent = "NexonLauncher.nxl-17.05.05-479-4a3d51e";
			reqPassport.Method = "GET";
			reqPassport.PreAuthenticate = true;
			reqPassport.Headers.Add("Authorization", "Bearer " + b64ApiToken);
			reqPassport.CookieContainer.Add(new Cookie("nxtk", b64ApiToken) { Domain = ".nexon.net" });
			WebResponse resp = reqPassport.GetResponse();
			Stream ds = resp.GetResponseStream();
			StreamReader read = new StreamReader(ds);
			String result = read.ReadToEnd();
			resp.Close();
			ds.Close();
			read.Close();
			return result;
		}*/

		public string getNxPassport()
		{
			if (apiToken == null || DateTimeOffset.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds >= accessTokenExpires)
			{
				getAPIToken();
			}

			var request = new RestRequest("users/me/passport", Method.GET);
			request.AddHeader("Authorization", "Bearer " + apiToken);
			var response = client.Execute(request);
			Console.WriteLine("GET PASSPORT: " +response.Content );
			var body = JsonConvert.DeserializeObject<dynamic>(response.Content);
			return body["passport"];
		}

		public int getVersion()
		{
			if (apiToken==null || DateTimeOffset.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds >= accessTokenExpires)
			{
				getAPIToken();
			}
			var request = new RestRequest("products/10200", Method.GET);
			request.AddHeader("Authorization", "Bearer " + apiToken);

			var response = client.Execute(request);
			Console.WriteLine("GET VERSION: " + response.Content);
			return 0;
		}

		public void getAPIToken()
		{
			var apiClient = new RestClient("https://accounts.nexon.net");
			apiClient.UserAgent = "NexonLauncher.nxl-17.05.05-479-4a3d51e";
			SHA512 sha = new SHA512Managed();
			
			var request = new RestRequest("account/login/launcher", Method.POST);
			var requestBody = new AccountLoginJson
			{
				id = server.username,
				password = BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(server.password))).Replace("-", "").ToLower(),
				auto_login = false,
				client_id = server.clientID,
				scope = "us.launcher.all",
				device_id = server.getUUID(),
			};
			request.AddJsonBody(requestBody);
			IRestResponse response = apiClient.Execute(request);
			var body = JsonConvert.DeserializeObject<dynamic>(response.Content);
			TimeSpan span = DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0));
			apiToken = body["access_token"];
			string expireTime = body["access_token_expires_in"];
			accessTokenExpires = span.TotalSeconds+Double.Parse(expireTime);
			//add to rest client cookies
			cookies.Add(new Cookie("nxtk", apiToken) { Domain = ".nexon.net" });
			Console.WriteLine("Access token get! (" + apiToken + ")");
		}

		private struct AccountLoginJson
		{
			public string id { get; set; }

			public string password { get; set; }

			public bool auto_login { get; set; }

			public string client_id { get; set; }

			public string scope { get; set; }

			public string device_id { get; set; }
		}
	}
}
