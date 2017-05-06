using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using IqFeed.Core;

namespace IqFeed {
	public class Admin {
		IqConnection admin = null;
		Thread reader = null;
		bool keepReading = true;


		public bool Connected { get; private set; }

		public Admin() {
			this.Connected = false;
		}
		public async Task<bool> Launch() {
			int retry = 0;
			string product = ConfigurationManager.AppSettings["Product"];
			string version = ConfigurationManager.AppSettings["Version"];

			LaunchFeed();
			while (admin == null) {
				await Task.Delay(1000);
				try {
					admin = new IqConnection("127.0.0.1", IQFeedConfiguration.IQAdminPort);
					admin.Connect();
				} catch (Exception e) {
					Console.WriteLine(e.Message);
					admin = null;
					if (retry++ > 3) {
						LaunchFeed();
					}
				}
			}
			await Register(product, version);
			await Connect();
			await SetProtocol();
			return true;
		}
		public async Task WriteCommand(string cmd) {
			await admin.WriteLine(cmd);
		}
		private async Task Register(string product, string version) {
			string cmd = $"S,REGISTER CLIENT APP,{product},{version}";
			string response;

			Console.WriteLine($"sent: {cmd}");

			await this.WriteCommand(cmd);
			response = await admin.ReadLine();

			Console.WriteLine($"rcvd: {response}");

			reader = new Thread(new ThreadStart(AdminStreamReader));
			reader.Start();


		}
		private async Task SetProtocol() {
			await this.WriteCommand("S,SET PROTOCOL,5.2");
		}
		private async Task Connect() {
			await this.WriteCommand("S,CONNECT");
		}

		private void LaunchFeed() {
			Process.Start("/Applications/iqfeed.app");
		}
		private void AdminStreamReader() {
			string line;
			using (StreamReader streamReader = new StreamReader(admin.Stream)) {
				while (keepReading == true) {
					line = streamReader.ReadLine();
					if (line.IndexOf("S,STATS", StringComparison.InvariantCulture) == 0) {
						if (line.IndexOf(",Connected,", StringComparison.InvariantCulture) != -1) {
							this.Connected = true;
						} else {
							this.Connected = false;
						}
					} else {
						Console.WriteLine($"rcvd: {line}");
					}
				}
			}
		}
	}
}