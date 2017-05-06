using System;
using System.Configuration;
using System.Data;
using Npgsql;
using System.Reactive.Subjects;

using IqFeed.Model;

namespace IqFeed {
	public class PostgresUpdater : IWriter {
		private Subject<TicksResponse> tickResponses;
		private Subject<IntervalsResponse> intervalResponses;
		private NpgsqlConnection postgres;

		private string user = ConfigurationManager.AppSettings["PostgresUser"];
		private string port = ConfigurationManager.AppSettings["PostgresPort"];
		private string dbname = ConfigurationManager.AppSettings["PostgresDatabase"];
		private string pass = ConfigurationManager.AppSettings["PostgresPassword"];

		private NpgsqlCommand insertTicks;
			
		public PostgresUpdater(Subject<TicksResponse> tickResponses, Subject<IntervalsResponse> intervalResponses) {
			this.tickResponses = tickResponses;
			this.intervalResponses = intervalResponses;

			this.tickResponses.Subscribe(
				(response) => TicksReponseHandler(response),
				() => Done()
			);

			this.intervalResponses.Subscribe(
				(response) => IntervalsResponseHandler(response),
				() => Done()
			);

			string connect = $"Server=127.0.0.1;Port={port};User Id={user};Password={pass};Database={dbname}";
			postgres = new NpgsqlConnection(connect);
			postgres.Open();

			NpgsqlDataAdapter adapter = new NpgsqlDataAdapter();
			adapter.SelectCommand = new NpgsqlCommand("SELECT * FROM \"historical_ticks\"", postgres);
			NpgsqlCommandBuilder builder = new NpgsqlCommandBuilder(adapter);
			insertTicks = builder.GetInsertCommand(true);
		}
		private void TicksReponseHandler(TicksResponse tick) {
			insertTicks.Parameters["symbol"].Value = tick.symbol;
			insertTicks.Parameters["time_stamp"].Value = tick.timeStamp.UtcValue;
			insertTicks.Parameters["last"].Value = tick.last;
			insertTicks.Parameters["last_size"].Value = tick.lastSize;
			insertTicks.Parameters["total_volume"].Value = tick.totalVolume;
			insertTicks.Parameters["bid"].Value = tick.bid;
			insertTicks.Parameters["ask"].Value = tick.ask;
			insertTicks.Parameters["tick_id"].Value = tick.tickID;
			insertTicks.Parameters["basis"].Value = tick.basis;
			insertTicks.Parameters["trade_market_center"].Value = tick.tradeMarketCenter;
			insertTicks.Parameters["trade_conditions"].Value = tick.tradeConditions;

			insertTicks.ExecuteNonQuery();
		}
		private void IntervalsResponseHandler(IntervalsResponse interval) {
			Console.WriteLine("IntervalsResponseHandler");
		}
		private void Done() {
		}
	}
}
