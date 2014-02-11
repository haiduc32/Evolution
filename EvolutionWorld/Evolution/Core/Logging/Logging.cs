using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Core.Logging
{
	public class Logging
	{
		//public void Log(LogSystem logSystem, LogLevel logLevel, string text)
		//{

		//}

		internal void LogEngine(string text)
		{

		}

		internal void LogStatistics(LogStatisticsType statisticsType)
		{

		}

		internal void LogError(LogSystem logSystem, Exception e)
		{

		}

		internal void LogUnit(int id, string text)
		{
			if (OnLogMessage != null)
			{
				OnLogMessage(this, string.Format("{0}: {1}", id, text));
			}
		}

		public event EventHandler<string> OnLogMessage;
	}
}
