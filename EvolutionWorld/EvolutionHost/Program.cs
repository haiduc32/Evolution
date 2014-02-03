using Evolution;
using Evolution.Events;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Hosting;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EvolutionHost
{
    class Program
    {
        private static bool _exit;
		private static Timer timer;
		private static Engine engine;

		public static List<VillagerNpc> AllVillagers { get; private set; }

        static void Main(string[] args)
        {
			timer = new Timer(OnTimer, null, 1000, 1000);
			

            Console.WriteLine("Starting engine.");
            engine = new Engine();
            engine.Start();
			AllVillagers = new List<VillagerNpc>();

			AllVillagers.AddRange(engine.CreateVillagers());

			foreach (VillagerNpc villager in AllVillagers)
			{
				//villager.OnBeginMove += VillagerBeginMove;
				villager.OnEndMove += VillagerEndMove;
				villager.OnBeginPath += VillagerBeginPath;
				villager.OnContinuePath += VillagerContinuePath;
				villager.OnEndPath += VillagerEndPath;
			}

			//TODO: should add only the new units here!
			NewUnits(AllVillagers);

			//start the hub
			string url = "http://localhost:9999";
			using (WebApp.Start(url))
			{

				Console.WriteLine("Server running on {0}", url);

				HttpClient client = new HttpClient();
				url = "http://partizan-server:9999";
				var response = client.GetAsync(url + "/signalr/hubs").Result;
				response = client.GetAsync(url + "/signalr/hubs").Result;

				Console.WriteLine(response);
				Console.WriteLine("Content:");
				Console.WriteLine(response.Content.ReadAsStringAsync().Result);



				Console.ReadKey();

			}
        }

		private static void NewUnits(List<VillagerNpc> units)
		{
			var hubContext = GlobalHost.ConnectionManager.GetHubContext<EvolutionHub>();

			var locations = units.Select(x =>
				new { id = x.Id, location = x.Location });
			hubContext.Clients.All.loadUnits(locations);
		}

		private static void VillagerBeginPath(object sender, UnitBeginPathEventArgs args)
		{
			var hubContext = GlobalHost.ConnectionManager.GetHubContext<EvolutionHub>();
			hubContext.Clients.All.unitBeginPath(args.Unit.Id, args.Path);
		}

		private static void VillagerContinuePath(object sender, UnitContinuePathEventArgs args)
		{
			var hubContext = GlobalHost.ConnectionManager.GetHubContext<EvolutionHub>();
			hubContext.Clients.All.unitContinuePath(args.Unit.Id, args.Path);
		}

		private static void VillagerEndPath(object sender, UnitEndPathEventArgs args)
		{
			var hubContext = GlobalHost.ConnectionManager.GetHubContext<EvolutionHub>();
			hubContext.Clients.All.unitEndPath(args.Unit.Id, args.Location, args.PathInterrupted);
		}

		//private static void VillagerBeginMove(object sender, UnitBeginMoveEventArgs args)
		//{
		//	var hubContext = GlobalHost.ConnectionManager.GetHubContext<EvolutionHub>();
		//	hubContext.Clients.All.unitBeginMove(args.Unit.Id, args.LastLocation, args.NewLocation);
		//}

		private static void VillagerEndMove(object sender, UnitEndMoveEventArgs args)
		{
			var hubContext = GlobalHost.ConnectionManager.GetHubContext<EvolutionHub>();
			hubContext.Clients.All.unitEndMove(args.Unit.Id, args.LastLocation, args.NewLocation);
		}

		private static void OnTimer(object state)
		{
			//int cursorTop = Console.CursorTop;
			//Console.WriteLine()
			//engine.PrintStats();
		}
    }
}
