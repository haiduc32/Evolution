using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvolutionHost
{
	public class EvolutionHub : Hub
	{
		public string Hello()
		{
			return "Welcome stranger.";
		}

		public override Task OnConnected()
		{
			var locations = Program.AllVillagers.Select(x => 
				new { id = x.Id, location = x.Location });
			Clients.Caller.loadUnits(locations);

			return base.OnConnected();
		}
	}
}
