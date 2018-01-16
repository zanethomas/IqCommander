using System;
using System.Collections.ObjectModel;
using System.Globalization;
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
                //HTT,AUDCAD.FXCM,20170820 220000,20170821 215959,,,,1
                string[] fnparts = parts[2].Split(',');

                fnparts[1] = fnparts[1].Replace('.', '_');

                filename = $"{fnparts[1]}.{fnparts[2].Replace(' ','_')}.{fnparts[3].Replace(' ','_')}";

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
            string[] parts = cmd.Split(',');
   //         parts[2] = ConvertToEst(parts[2]);
   //         parts[3] = ConvertToEst(parts[3]);

   //         Console.WriteLine("old: {0}", cmd);
   //         cmd = String.Join(",",parts);
			//Console.WriteLine("new: {0}", cmd);
			await history.WriteLine(cmd);
            await history.GetData(cmd, line => {
                if (line != null) {
                    Responses.OnNext(line);
                    TickResponses.OnNext(new TicksResponse(parts[1], line));
                } else {
                    csvwriter.Dispose();
                    Responses.OnCompleted();
                }
            });
        }
        // 20170821 000000
        private string ConvertToEst(string utcDate) {
            string year = utcDate.Substring(0, 4);
            string month = utcDate.Substring(4, 2);
            string day = utcDate.Substring(6, 2);
            string hour = utcDate.Substring(9, 2);
            string minute = utcDate.Substring(11, 2);
            string second = utcDate.Substring(13, 2);

            string utcString = String.Format("{0:D4}-{1:D2}-{2:D2}T{3:D2}:{4:D2}:{5:D2}.000Z", year, month, day, hour, minute, second);
            DateTime utcDateTime = DateTime.Parse(utcString, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

#if false
            ReadOnlyCollection<TimeZoneInfo> timezones = TimeZoneInfo.GetSystemTimeZones();

            foreach (TimeZoneInfo zinfo in timezones) {
                Console.WriteLine(zinfo.Id);
            }
#endif
            TimeZoneInfo easternTimeZone = TimeZoneInfo.FindSystemTimeZoneById("US/Eastern");

			DateTime easternDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime,
																	   easternTimeZone);
            return String.Format("{0:D4}{1:D2}{2:D2} {3:D2}{4:D2}{5:D2}", easternDateTime.Year, easternDateTime.Month, easternDateTime.Day, easternDateTime.Hour, easternDateTime.Minute, easternDateTime.Second);

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
