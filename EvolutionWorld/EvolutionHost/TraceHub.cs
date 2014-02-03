using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvolutionHost
{
	public class TraceHub : Hub
	{
		public void Trace(bool enabled)
		{
			TraceConsole.Instance.Start();
		}

		public void LogEntity(int entityId, bool enabled)
		{

		}
	}
}
