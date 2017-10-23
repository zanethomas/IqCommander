using System;
using System.IO;
using System.Reactive.Subjects;
using System.Threading.Tasks;

using IqFeed.Core;
using IqFeed.Model;



namespace IqFeed.Commanders {
	public abstract class CommanderBase {
		internal IqConnection history;
		internal Action<string> requestTracer;
		internal Action<string> responseTracer;
		internal Admin admin;

		public Subject<string> Responses = new Subject<string>();
		public Subject<TicksResponse> TickResponses = new Subject<TicksResponse>();
		public Subject<IntervalsResponse> IntervalResponses = new Subject<IntervalsResponse>();
		public string CommandType { get; private set; }
		private PostgresUpdater postgres = null;
		private CsvWriter csvwriter = null;
		private bool writeCsv = false;


		public CommanderBase() {
			history = new IqConnection("127.0.0.1", IQFeedConfiguration.IQFeedHistoryPort);
		}
		public abstract Task Run(Admin admin);
		internal void Connect() {
			history.Connect();
		}

		internal async Task Execute(string cmd) {
			string[] parts = cmd.Split('|');

			switch (parts[0]) {
				case "csv":
					writeCsv = true;
					break;
				case "postgres":
					postgres = new PostgresUpdater(TickResponses, IntervalResponses);
					break;
				case "history":
					await ExecuteHistoryCommand(parts);
					break;
				case "quit":
					break;
			}
			return;
		}

		private async Task ExecuteHistoryCommand(string[] parts) {
			string type = parts[1];
			string command = parts[2];
			string filename = "";

			if (writeCsv) {
				int i = command.IndexOf(',') + 1;

				filename = command.Substring(i, 11);

				TickResponses = new Subject<TicksResponse>();
		        IntervalResponses = new Subject<IntervalsResponse>();

		        csvwriter = new CsvWriter(TickResponses, IntervalResponses, filename);
			}

			if (type == "run") {
				await RunHistoryCommand(parts[2]);
			} else if (type == "write") {
				await history.WriteLine(parts[2]);
			} else if (type == "expect") {
				await ExpectResponse(parts[2]);
			}
		}
		private async Task<bool> ExpectResponse(string response) {
			string got = await history.ReadLine();
			return response == got;
		}
		private async Task RunHistoryCommand(string command) {
			int commaAt = command.IndexOf(',');

			if (commaAt != -1) {
				CommandType = command.ToUpper().Substring(0, command.IndexOf(','));
			} else {
				CommandType = command.ToUpper();
			}
			switch (CommandType) {
				case "HTX":
				case "HTD":
				case "HTT":
					await HistoryTicks(command);
					break;
				case "HIX":
				case "HID":
				case "HIT":
					await HistoryIntervals(command);
					break;
				case "HDX":
				case "HDT":
				case "HWX":
				case "HMX":
					break;

			}

		}
		private void ExecuteControl(string cmd) {
			string[] parts = cmd.Split(' ');
			if (parts[0] == "postgres") {
				postgres = new PostgresUpdater(TickResponses, IntervalResponses);
			}
			return;

		}
		private async Task HistoryTicks(string cmd) {
			var parts = cmd.Split(',');
			await history.WriteLine(cmd);
			await history.GetData(cmd, line => {
                if(line != null) {
					Responses.OnNext(line);
					TickResponses.OnNext(new TicksResponse(parts[1], line));
                } else {
                    csvwriter.Dispose();
                    Responses.OnCompleted();
                }
			});
		}

		private async Task HistoryIntervals(string cmd) {
			var parts = cmd.Split(',');
			await history.WriteLine(cmd);
			await history.GetData(cmd, line => {
				Responses.OnNext(line);
				IntervalResponses.OnNext(new IntervalsResponse(parts[1], line));
			});
		}
		private object HistoryDates(string cmd) {
			return new object();
		}

	}
}
