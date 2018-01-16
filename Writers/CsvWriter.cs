using System;
using System.IO;
using System.Configuration;
using System.Data;
using Npgsql;
using System.Reactive.Subjects;

using System.Globalization;
using System.Text.RegularExpressions;

using IqFeed.Model;

namespace IqFeed {
	public class CsvWriter : IWriter, IDisposable {
		private Subject<TicksResponse> tickResponses;
		private Subject<IntervalsResponse> intervalResponses;
		private string filename;
		private StreamWriter writer;
		private string fmtpattern = "yyyy-MM-dd HH:mm:ss.fff";





		public CsvWriter(Subject<TicksResponse> tickResponses, Subject<IntervalsResponse> intervalResponses, string filename) {
			this.tickResponses = tickResponses;
			this.intervalResponses = intervalResponses;
			this.filename = filename;
			this.writer = null;

			this.tickResponses.Subscribe(
				(response) => TicksReponseHandler(response),
				() => Done()
			);

			this.intervalResponses.Subscribe(
				(response) => IntervalsResponseHandler(response),
				() => Done()
			);


		}
		private void TicksReponseHandler(TicksResponse tick) {
            string timestamp = tick.timeStamp.OriginalValue.ToString(this.fmtpattern);
			if (this.writer == null) {
				this.writer = new StreamWriter(filename + ".csv");
				writer.WriteLine("symbol,timestamp,last,last_size,total_volume,bid,ask,tick_id,basis,trade_market_center,trace_conditions");
			}
			writer.WriteLine($"{tick.symbol},{timestamp},{tick.last},{tick.lastSize},{tick.totalVolume},{tick.bid},{tick.ask},{tick.tickID},{tick.basis},{tick.tradeMarketCenter},{tick.tradeConditions}");
		}

		private void IntervalsResponseHandler(IntervalsResponse interval) {
			//insertSeconds.Parameters["symbol"].Value = interval.symbol;
			//insertSeconds.Parameters["time_stamp"].Value = interval.timeStamp.UtcValue;
			//insertSeconds.Parameters["high"].Value = interval.high;
			//insertSeconds.Parameters["low"].Value = interval.low;
			//insertSeconds.Parameters["open_value"].Value = interval.open;
			//insertSeconds.Parameters["close_value"].Value = interval.close;
			//insertSeconds.Parameters["period_volume"].Value = interval.periodVolume;
			//insertSeconds.Parameters["total_volume"].Value = interval.totalVolume;
			//insertSeconds.Parameters["number_of_trades"].Value = interval.numberOfTrades;

			//insertSeconds.ExecuteNonQuery();
		}
		private void Done() {
			writer.Close();
            this.tickResponses.Dispose();
		}

		public void Dispose() {
			this.Done();
		}
	}
}
