using System;
using System.IO;
using System.Net.Sockets;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace IqFeed.Core {
	public class IqConnection {
		private NetworkStream stream = null;
		private StreamReader reader = null;
		private string host;
		private int port;

		public Subject<string> Trace = new Subject<string>();

		public IqConnection(string host, int port) {
			this.host = host;
			this.port = port;
		}
		public void Connect() {
			//
			// TODO fails when iqfeed app not running, of course
			//
			TcpClient socket = new TcpClient(this.host, this.port);

			this.stream = new NetworkStream(socket.Client);
		}
		public NetworkStream Stream {
			get { return this.stream; }
		}
		public async Task WriteLine(string line) {
			if (Trace.HasObservers == true) {
				Trace.OnNext($"s: {line}");
			}
			if (stream.CanWrite) {
				byte[] data = Encoding.ASCII.GetBytes(line + "\r\n");

				await stream.WriteAsync(data, 0, data.Length);
				await stream.FlushAsync();
				this.reader = null;
			} else {
				throw new IOException("could not write to stream");
			}
		}
		public async Task<string> ReadLine() {
			if (this.reader == null) {
				this.reader = new StreamReader(stream);
			}
			return await reader.ReadLineAsync();
		}

		public async Task GetData(string request, Action<string> process) {
			if (this.reader == null) {
				this.reader = new StreamReader(stream);
			}
			await ProcessData(reader, process);
		}


		private async Task ProcessData(StreamReader streamReader, Action<string> callback) {
			var line = await streamReader.ReadLineAsync();
			if (ResponseIsOk(line) != true) {
				throw new Exception("Did not want to see " + line);
			}
			while (!streamReader.EndOfStream) {
				line = await streamReader.ReadLineAsync();
				if (CheckEndMessage(line) == true) {
					break;
				}
				callback(line);
			}
		}

		private bool CheckEndMessage(string line) {
			return (line == null) ||
						string.IsNullOrEmpty(line) ||
					  	line.Contains(IQFeedConfiguration.EndMessage);
		}

		private bool ResponseIsOk(string line) {
			var isValidContent = true;

			if (!string.IsNullOrEmpty(line)) {
				if (line.Contains(IQFeedConfiguration.NoDataError)) {
					isValidContent = false;
				}

				if (line.Contains(IQFeedConfiguration.InvalidSymbolError)) {
					isValidContent = false;
				}

				if (line.Contains(IQFeedConfiguration.SyntaxError)) {
					isValidContent = false;
				}

				if (line.Contains(IQFeedConfiguration.CantConnectHistorySocket)) {
					isValidContent = false;
				}

				if (line.Contains(IQFeedConfiguration.CantConnectSymbolLookupSocket)) {
					isValidContent = false;
				}
			}
			return isValidContent;
		}

		public TcpClient OpenTcpClientConnection(string host, int port) {
			TcpClient socket = null;

			try {
				socket = new TcpClient(host, port);
			} catch (SocketException) {

			}

			return socket;
		}

		public NetworkStream CreateStream(TcpClient socket) {
			if (socket == null) throw new ArgumentNullException(nameof(socket));

			NetworkStream netStream = socket.GetStream();
			return netStream;
		}

	}
	public static class IQFeedConfiguration {
		public static string NoDataError = "NO_DATA";
		public static string EndMessage = "ENDMSG";
		public static string SyntaxError = "SYNTAX_ERROR";
		public static string Current52Protocol = "S,CURRENT PROTOCOL,5.2";
		public static string InvalidSymbolError = "Invalid symbol";
		public static string CantConnectHistorySocket = "Could not connect to History socket";
		public static string CantConnectSymbolLookupSocket = "Could not connect to SymbolLookup socket";
		public static string IQFeedHostName = "127.0.0.1";
		public static int IQFeedHistoryPort = 9100;
		public static int IQFeedLevel1Port = 5009;
		public static int IQAdminPort = 9300;


		public static byte EOL = 0x0;

		public static string Delimiter = ",";
		public static string Terminater = Environment.NewLine;

		public static string TickDaysHeader = "HTD";
		public static string TickIntervalHeader = "HTT";
		public static string IntradayDaysHeader = "HID";
		public static string IntradayIntervalHeader = "HIT";
		public static string DailyDaysHeader = "HDX";
		public static string DailyIntervalHeader = "HDT";
		public static string WeeklyDaysHeader = "HWX";
		public static string MonthlyDaysHeader = "HMX";
	}
}
