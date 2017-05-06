using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace IqFeed.Commanders {
	public class FileCommander : CommanderBase {
		private string filename;

		public FileCommander(string filename) : base() {
			this.filename = filename;
		}

		public async override Task Run(Admin admin) {
			StreamReader file = new StreamReader(this.filename);
			string line;

			base.admin = admin;

			while (admin.Connected == false) {
				Console.WriteLine("waiting for admin connected");
				await Task.Delay(1000);
			}

			base.Connect();

			while (file.EndOfStream == false) {
				line = await file.ReadLineAsync();
				line = line.Trim();
				if (line =="quit") {
					break;
				}
				if (line.TrimStart().IndexOf('#') != 0) {
					Console.WriteLine($"executing {line}");
					await base.Execute(line);
				}
			}
			file.Close();
			Environment.Exit(0);
		}
	}
}
