using System;
using IqFeed.Core;

namespace IqFeed
{
	public class CommandLine : CommanderBase
	{
		public CommandLine(Action<string> RequestTracer, Action<string> ResponseTracer) : base(RequestTracer, ResponseTracer)
		{
		}
		public void Run()
		{
			string cmd = "";

			while (true) {
				try {
					
					history.Connect();
					history.SetProtocol();

					Console.Write("cmd <- ");
					cmd = Console.ReadLine().Trim();

					if (!String.IsNullOrWhiteSpace(cmd) && cmd[0] != '\0') {
						if (cmd.ToLower().TrimStart().IndexOf("quit", StringComparison.CurrentCulture) == 0) {
							return;
						}
						Execute(cmd);
					}
				} catch (Exception e) {
					Console.Error.WriteLine("Error {0} processing command {1}", e.Message, cmd);
				} 
			}
		}
	}
}
