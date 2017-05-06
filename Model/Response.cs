using System;
namespace IqFeed.Model
{
	public abstract class Response
	{
		public string text;
		public string symbol;
		public bool valid;

		public Response(string symbol, string text)
		{
			this.text = text;
			this.symbol = symbol;
			valid = true;
		}
		abstract public string Type { get; }
		abstract public string[] ColumnNames { get; }
	}
}
