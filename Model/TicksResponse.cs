using System;
using IqFeed.Core;

namespace IqFeed.Model
{
	public class TicksResponse : Response
	{
		public TicksResponse(string symbol, string text) : base(symbol, text) {
			string[] parts = text.Split(',');

			timeStamp = new TimeStamp(parts[0]);
			Decimal.TryParse(parts[1], out last);
			int.TryParse(parts[2], out lastSize);
			int.TryParse(parts[3], out totalVolume);
			Decimal.TryParse(parts[4], out bid);
			Decimal.TryParse(parts[5], out ask);
			int.TryParse(parts[6], out tickID);
			basis = parts[7];
			int.TryParse(parts[8], out tradeMarketCenter);
			tradeConditions = parts[9];
			                   
		}

		public override  string Type {
			get { return this.type; }
		}
		public override string[] ColumnNames {
			get { return this.columnNames; }
		}
		private string type = "ticks";
		private string[] columnNames = new string[] { "TimeStamp","Last","LastSize","TotalVolume","Bid","Ask","TickID","Basis","MarketCenter","TraceConditions" };
		public TimeStamp timeStamp;
		public decimal last;
		public int lastSize;
		public int totalVolume;
		public decimal bid;
		public decimal ask;
		public int tickID;
		public string basis;
		public int tradeMarketCenter;
		public string tradeConditions;

	}
}
