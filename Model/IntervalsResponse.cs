using System;
using IqFeed.Core;

namespace IqFeed.Model
{
	public class IntervalsResponse : Response
	{
		public IntervalsResponse(string symbol, string text) : base(symbol, text)
		{
			string[] parts = text.Split(',');

			if (parts.Length != 9) {
				valid = false;
				return;
			}
			timeStamp = new TimeStamp(parts[0]);
			valid &= Decimal.TryParse(parts[1], out high);
			valid &= Decimal.TryParse(parts[2], out low);
			valid &= Decimal.TryParse(parts[3], out open);
			valid &= Decimal.TryParse(parts[4], out close);
			valid &= int.TryParse(parts[5], out totalVolume);
			valid &= int.TryParse(parts[6], out periodVolume);
			valid &= int.TryParse(parts[7], out numberOfTrades);
		}
		public override string Type {
			get {
				return this.type;
			}
		}
		public override string[] ColumnNames {
			get { return this.columnNames; }
		}

		private string type = "intervals";
		private string[] columnNames = new string[] { "TimeStamp","High","Low","Open","Close","TotalVolume","PeriodVolume","NumberOfTrades"};
		public TimeStamp timeStamp;
		public int totalVolume;
		public decimal high;
		public decimal low;
		public decimal open;
		public decimal close;
		public int periodVolume;
		public int numberOfTrades;
	}
}
