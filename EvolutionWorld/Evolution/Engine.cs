using Evolution.Characters;
using Evolution.Core;
using Evolution.Core.Logging;
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

		private readonly ConcurrentDictionary<UnitBase, ActionBase> _unitActionDict;

		private readonly ConcurrentDictionary<long, ConcurrentQueue<UnitBase>> _scheduledActionsDict;

		private readonly Map _map;

		private readonly TimeKeeper _timeKeeper;

		private readonly List<UnitBase> _characters;


		#endregion private fields

		#region properties

		public Map Map { get { return _map; } }

		public Logging Logging { get; private set; }

		#endregion properties

		#region .ctor

		public Engine()
		{
			_unitActionDict = new ConcurrentDictionary<UnitBase, ActionBase>();
			_scheduledActionsDict = new ConcurrentDictionary<long, ConcurrentQueue<UnitBase>>();
			_timeKeeper = new TimeKeeper();
			_map = new Map();
			_characters = new List<UnitBase>();


			Logging = new Logging();
		}

		#endregion .ctor

		#region just for testing
		List<UnitBase> villagers;
		public void PrintStats()
		{
			foreach (UnitBase villager in villagers)
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
				Id = _characters.Any() ? _characters.Max(unit => unit.Id) + 1 : 0,
				Logging = Logging
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
						if (_scheduledActionsDict.TryGetValue(tick, out listOfActions))
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

							//now remove the list of actions because they have been processed
							_scheduledActionsDict.TryRemove(tick, out listOfActions);
						}
					}
					catch (Exception e)
					{
						//todo: implement logging
					}
				};

			_timeKeeper.Start();
		}

		#region unit acttions

		public bool ProcessIdle(UnitBase unit, int idleTicks)
		{
			if (_unitActionDict.ContainsKey(unit)) return false;

			IdleAction idleAction = new IdleAction
			{
				Tick = _timeKeeper.Tick + idleTicks
			};

			AddAction(unit, idleAction);

			return true;
		}

		public bool ProcessMove(UnitBase unit, Location nextLeg)
		{
			Debug.WriteLineIf(unit.Id == 0, "Engine: ProcessMove");
			if (_unitActionDict.ContainsKey(unit))
			{
				Debug.WriteLineIf(unit.Id == 0, "Engine: ProcessMove returning false");
				return false;
			}

			return ProcessMoveInternal(unit, nextLeg);
		}

		public void ProcessAttack(UnitBase attackignUnit, UnitBase attackedUnit)
		{
			//TODO: check that the attacked unit is in range

			throw new NotImplementedException();
		}

		public void CancelAllActions(UnitBase unit)
		{
			throw new NotImplementedException();
		}

		#endregion unit actions
		
		#region private methods
		
		/// <summary>
		/// We need an internal ProcessMove method to avoid altering the original path.
		/// </summary>
		/// <param name="unit"></param>
		/// <param name="nextLeg"></param>
		/// <returns></returns>
		private bool ProcessMoveInternal(UnitBase unit, Location nextLeg)
		{
			//check that the map is empty at the desired location
			if (!_map.ReserveIfEmpty(nextLeg, unit)) return false;

			MoveAction moveAction = new MoveAction
			{
				Tick = _timeKeeper.Tick + unit.MovementSpeed,
				Location = nextLeg
			};

			unit.BeginMove(nextLeg);

			AddAction(unit, moveAction);

			return true;
		}

		private void ActionHandler(UnitBase unit)
		{
			try
			{
				ActionBase action;
				if (_unitActionDict.TryRemove(unit, out action))
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
			Location oldLocation = unit.Location;
			Location newLocation = action.Location;
			_map.Move(unit, oldLocation, newLocation);

			unit.EndMove(action.Location);
			unit.Ready(ActionType.Move);
		}

		private void IdleActionHandler(UnitBase unit, IdleAction action)
		{
			unit.Ready(action.ActionType);
		}

		private void AddAction(UnitBase unit, ActionBase action)
		{
			_unitActionDict.TryAdd(unit, action);

			ConcurrentQueue<UnitBase> actionList = _scheduledActionsDict
				.GetOrAdd(action.Tick, new ConcurrentQueue<UnitBase>());
			actionList.Enqueue(unit);
		}

		#endregion private methods

	}
}
