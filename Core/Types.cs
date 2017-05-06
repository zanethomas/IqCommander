using System;
namespace IqFeed.Core
{
	public class TimeStamp
	{
		DateTime timeStampOriginal;
		DateTime timeStampUTC;
		public TimeStamp(string data)
		{
			TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById("EST");

			timeStampOriginal = Convert.ToDateTime(data);

			timeStampUTC = TimeZoneInfo.ConvertTimeToUtc(timeStampOriginal, tzi);
		}
		public DateTime OriginalValue {
			get { return this.timeStampOriginal; }
		}
		public DateTime UtcValue {
			get { return this.timeStampUTC; }
		}
	}
}
