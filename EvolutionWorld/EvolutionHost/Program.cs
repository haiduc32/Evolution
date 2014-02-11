using Evolution;
using Evolution.Characters;
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
			engine.Logging.OnLogMessage += Logging_OnLogMessage;
            engine.Start();
			AllVillagers = new List<VillagerNpc>();

			AllVillagers.AddRange(engine.CreateVillagers());

			//AllVillagers.Add(engine.CreateVillager(0, 0));

			foreach (VillagerNpc villager in AllVillagers)
			{
				//villager.OnBeginMove += VillagerBeginMove;
				villager.OnEndMove += VillagerEndMove;
				villager.OnBeginPath += VillagerBeginPath;
				//villager.OnContinuePath += VillagerContinuePath;
				villager.OnEndPath += VillagerEndPath;
			}

			NewUnits(AllVillagers);
			string hostUrl = System.Configuration.ConfigurationManager.AppSettings["hostUrl"];

			//start the hub
			using (WebApp.Start(hostUrl))
			{

				//Console.WriteLine("Server running on {0}", hostUrl);

				//HttpClient client = new HttpClient();
				////url = "http://partizan-server:9999";
				//var response = client.GetAsync(hostUrl + "/signalr/hubs").Result;
				//if (response.StatusCode != System.Net.HttpStatusCode.OK)
				//{
				//	Console.WriteLine("Could not check that the port has been opened with success.");
				//}
				//else
				//{
				//	Console.WriteLine("Running OK.");
				//}



				Console.ReadKey();

			}
        }

		static void Logging_OnLogMessage(object sender, string e)
		{
			IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<EvolutionHub>();

			if (hubContext == null) return;

			hubContext.Clients.All.logMessage(e);
		}

		private static void NewUnits(IEnumerable<VillagerNpc> units)
		{
			IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<EvolutionHub>();

			var locations = units.Select(x =>
				new { id = x.Id, location = x.Location });
			hubContext.Clients.All.loadUnits(locations);
		}

		private static void VillagerBeginPath(object sender, UnitBeginPathEventArgs args)
		{
			IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<EvolutionHub>();
			hubContext.Clients.All.unitBeginPath(args.Unit.Id, args.Path);
		}

		private static void VillagerContinuePath(object sender, UnitContinuePathEventArgs args)
		{
			IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<EvolutionHub>();
			hubContext.Clients.All.unitContinuePath(args.Unit.Id, args.Path);
		}

		private static void VillagerEndPath(object sender, UnitEndPathEventArgs args)
		{
			IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<EvolutionHub>();
			hubContext.Clients.All.unitEndPath(args.Unit.Id, args.Location, args.PathInterrupted);
		}

		//private static void VillagerBeginMove(object sender, UnitBeginMoveEventArgs args)
		//{
		//	var hubContext = GlobalHost.ConnectionManager.GetHubContext<EvolutionHub>();
		//	hubContext.Clients.All.unitBeginMove(args.Unit.Id, args.LastLocation, args.NewLocation);
		//}

		private static void VillagerEndMove(object sender, UnitEndMoveEventArgs args)
		{
			IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<EvolutionHub>();
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
