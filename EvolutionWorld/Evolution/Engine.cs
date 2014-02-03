using Evolution.Core;
using Evolution.EngineActions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Evolution
{
	public class Engine
	{
		[DllImport("kernel32.dll")]
		public static extern long GetTickCount();

		#region private fields

		//private long _tick;

		//private int lastEnvironmentTickcount;

		private ConcurrentDictionary<UnitBase, ActionBase> unitActionDict;

		private ConcurrentDictionary<long, ConcurrentQueue<UnitBase>> scheduledActionsDict;

		private Map _map;

		private TimeKeeper _timeKeeper;

		private List<UnitBase> _characters;

		#endregion private fields

		#region properties

		public Map Map { get { return _map; } }

		#endregion properties

		#region .ctor

		public Engine()
		{
			unitActionDict = new ConcurrentDictionary<UnitBase, ActionBase>();
			scheduledActionsDict = new ConcurrentDictionary<long, ConcurrentQueue<UnitBase>>();
			_timeKeeper = new TimeKeeper();
			_map = new Map();
			_characters = new List<UnitBase>();
		}

		#endregion .ctor

		#region just for testing
		List<UnitBase> villagers;
		public void PrintStats()
		{
			foreach (VillagerNpc villager in villagers)
			{
				Console.WriteLine("{0} {1}", villager.Location.X, villager.Location.Y);
			}
		}

		public VillagerNpc CreateVillager(int x, int y)
		{
			Location location = new Location { X = x, Y = y, Z = 0 };

			//just skip to the next cell if the location is not empty
			if (!_map.IsEmptyCell(location)) return null;

			VillagerNpc villager = new VillagerNpc
			{
				Map = _map,
				Engine = this,
				Id = _characters.Any() ? _characters.Max(unit => unit.Id) + 1 : 0
			};

			_map.Add(villager, location);

			_characters.Add(villager);

			villager.EndMove(location);
			villager.Ready(ActionType.Idle);

			return villager;
		}

		/// <summary>
		/// Will create 10 villagers and spread them randomly on the map.
		/// </summary>
		public IEnumerable<VillagerNpc> CreateVillagers()
		{
			villagers = new List<UnitBase>();

			for (int i = 0; i < 10; i++)
			{
				VillagerNpc villager = CreateVillager(Random.Next(_map.MapHeight), Random.Next(_map.MapWidth));

				if (villager == null) continue;

				villagers.Add(villager);
			}

			return villagers.Select(x => (VillagerNpc)x);
		}

		#endregion just for testing

		public void Start()
		{
			_timeKeeper.OnTick = tick =>
				{
					try
					{
						ConcurrentQueue<UnitBase> listOfActions;
						if (scheduledActionsDict.TryGetValue(tick, out listOfActions))
						{
							ActionBlock<UnitBase> actionBlock = new ActionBlock<UnitBase>(x => ActionHandler(x));
							foreach (UnitBase unit in listOfActions)
							{
								actionBlock.Post(unit);
							}

							//tell action block to not accept any more actions
							actionBlock.Complete();
							//wait for all the actions to be completed
							actionBlock.Completion.Wait();
						}
					}
					catch (Exception e)
					{
						//todo: implement logging
					}
				};

			_timeKeeper.Start();
		}

		public bool ProcessIdle(UnitBase unit, int idleTicks)
		{
			if (unitActionDict.ContainsKey(unit)) return false;

			IdleAction idleAction = new IdleAction
			{
				Tick = _timeKeeper.Tick + idleTicks
			};

			AddAction(unit, idleAction);

			return true;
		}

		public bool ProcessMove(UnitBase unit, List<Location> path)
		{
			Debug.WriteLineIf(unit.Id == 0, "Engine: ProcessMove");
			if (unitActionDict.ContainsKey(unit))
			{
				Debug.WriteLineIf(unit.Id == 0, "Engine: ProcessMove returning false");
				return false;
			}

			return ProcessMoveInternal(unit, new List<Location>(path));
		}

		/// <summary>
		/// We need an internal ProcessMove method to avoid altering the original path.
		/// </summary>
		/// <param name="unit"></param>
		/// <param name="path"></param>
		/// <returns></returns>
		private bool ProcessMoveInternal(UnitBase unit, List<Location> path)
		{
			//check that the map is empty at the desired location
			if (!_map.ReserveIfEmpty(path[0], unit)) return false;

			MoveAction moveAction = new MoveAction
			{
				Tick = _timeKeeper.Tick + unit.MovementSpeed,
				Location = path[0],
				Path = path
			};

			unit.BeginMove(path[0], path.Count);

			AddAction(unit, moveAction);

			return true;
		}

		private void ActionHandler(UnitBase unit)
		{
			try
			{
				ActionBase action;
				if (unitActionDict.TryRemove(unit, out action))
				{
					switch (action.ActionType)
					{
						case ActionType.Move:
							MoveActionHandler(unit, (MoveAction)action);
							break;
						case ActionType.Idle:
							IdleActionHandler(unit, (IdleAction)action);
							break;
					};
				}
			}
			catch (Exception e)
			{

			}
		}

		private void MoveActionHandler(UnitBase unit, MoveAction action)
		{
			//condition removed because the reservation has been implemented
			//if (_map.IsEmptyCell(action.Location))
			//{
			//	Debug.WriteLineIf(unit.Id == 0, "Engine: Cell was not blocked");

			_map.Move(unit, unit.Location, action.Location);

			unit.EndMove(action.Location);

			action.Path.RemoveAt(0);

			if (action.Path.Count == 0)
			{
				unit.Ready(action.ActionType);
			}
			else
			{
				Debug.WriteLineIf(unit.Id == 0, "Engine: Moving to the next cell in path");
				//next action should be programmed
				//TODO: this logic might be better suited for the UnitBase
				if (!ProcessMoveInternal(unit, action.Path))
				{
					unit.Ready(action.ActionType);
				}
			}
			//}
			//else
			//{
			//	Debug.WriteLineIf(unit.Id == 0, "Engine: Cell was blocked");
			//	//the move failed because somebody already occupied the cell
			//	//but we need to inform the unit that he can perform new actions
			//	unit.Ready(action.ActionType);
			//}
		}

		private void IdleActionHandler(UnitBase unit, IdleAction action)
		{
			unit.Ready(action.ActionType);
		}

		private void AddAction(UnitBase unit, ActionBase action)
		{
			unitActionDict.TryAdd(unit, action);

			ConcurrentQueue<UnitBase> actionList = scheduledActionsDict
				.GetOrAdd(action.Tick, new ConcurrentQueue<UnitBase>());
			actionList.Enqueue(unit);
		}


	}
}
