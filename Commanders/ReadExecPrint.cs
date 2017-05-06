using System;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;


namespace IqFeed.Commanders
{
	public class ReadExecPrint : CommanderBase
	{
		StreamWriter stdout;
		StreamReader stdin;

		public ReadExecPrint() : base()
		{
			stdout = new StreamWriter(Console.OpenStandardOutput());
			stdin = new StreamReader(Console.OpenStandardInput());
		}
		public async override Task Run(Admin admin)
		{
			base.admin = admin;
			string cmd = "";

			while (admin.Connected == false) {
				Console.WriteLine("waiting for admin connected");
				await Task.Delay(1000);
			}
			while (true) {
				try {
					base.Connect();

					await stdout.WriteAsync("cmd <- ");
					await stdout.FlushAsync();
					cmd = await stdin.ReadLineAsync();
					cmd = cmd.Trim();
					if (!String.IsNullOrWhiteSpace(cmd) && cmd[0] != '\0') {
						if (cmd.ToLower().TrimStart().IndexOf("quit", StringComparison.CurrentCulture) == 0) {
							Environment.Exit(1);
							return;
						}
						await base.Execute(cmd);
					}
				} catch (Exception e) {
					Console.Error.WriteLine("Error {0} processing command {1}", e.Message, cmd);
				} 
			}
		}
	}
}
