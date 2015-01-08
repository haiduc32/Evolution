using Algorithms;
using Evolution.Core.Logging;
using Evolution.EngineActions;
using Evolution.Events;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Evolution.Characters
{
	public abstract class CharacterBase : Unit, IUnitEngineEvents, ICommunicationUnit
	{
		#region properties

		internal Logging Logging { get; set; }

		/// <summary>j
		/// Gets the Cartesian sensory range in number of cells.
		/// </summary>
		public abstract int Range { get; }

		/// <summary>
		/// Gets the Cartesian attack range.
		/// </summary>
		public virtual int AttackRange { get { return 1; } }

		public abstract bool IsNpc { get; }

		/// <summary>
		/// Ticks necessary to move to an adiacent cell. (1/100 of a sec)
		/// </summary>
		public abstract int MovementSpeed { get; }

		public int Life { get; private set; }

		public int MaxLife { get; private set; }

		

		public int ExperiencePoints { get; private set; }

		public CharacterBase AttackedUnit { get; protected set; }

		public bool IsFollowingRoute
		{
			get
			{
				return Destination.HasValue && Location != Destination;
			}
		}
		public Location? Destination { get; private set; }

		/// <summary>
		/// Gets or sets the remaning route legs.
		/// </summary>
		public List<Location> RemainingRoute { get; private set; }

		/// <summary>
		/// Gets or sets the number of legs of the calculated route.
		/// This is not the same as the remaining route legs count.
		/// </summary>
		protected int RouteTotalLegsCount { get; set; }

		public Engine Engine { get; set; }

		public Map Map { get; set; }

		public int Id { get; set; }

		protected List<CharacterBase> UnitsInRange { get; private set; } 

		#endregion properties

		#region events

		public EventHandler<UnitEventArgs> OnDie;

		public EventHandler<UnitBeginMoveEventArgs> OnBeginMove;

		public EventHandler<UnitEndMoveEventArgs> OnEndMove;

		public EventHandler<UnitBeginPathEventArgs> OnBeginPath;

		public EventHandler<UnitContinuePathEventArgs> OnContinuePath;

		public EventHandler<UnitEndPathEventArgs> OnEndPath;

		#endregion events

		#region .ctor

		public CharacterBase()
		{
			UnitsInRange = new List<CharacterBase>();
		}

		#endregion .ctor

		#region engine events

		/// <summary>
		/// Engine event. Called when a new unit comes in range.
		/// </summary>
		public virtual void UnitInRange(CharacterBase unit)
		{
			UnitsInRange.Add(unit);
		}

		/// <summary>
		/// Engine event. Called when a unit gets out of range (or also if dies).
		/// </summary>
		public virtual void UnitOutOfRange(CharacterBase unit)
		{
			UnitsInRange.Remove(unit);
		}

		/// <summary>
		/// Engine event. Notifies the unit that it's starting to move.
		/// </summary>
		/// <param name="targetLocation">Can be only an adiacent cell.</param>
		public virtual void BeginMove(Location targetLocation)
		{
			if (OnBeginMove != null)
			{
				UnitBeginMoveEventArgs args = new UnitBeginMoveEventArgs
				{
					Unit = this,
					LastLocation = Location,
					NewLocation = targetLocation
				};
				OnBeginMove(this, args);
			}

			if (RouteTotalLegsCount == RemainingRoute.Count)
			{
				if (OnBeginPath != null)
				{
					UnitBeginPathEventArgs args = new UnitBeginPathEventArgs
					{
						Unit = this,
						Path = RemainingRoute
					};

					OnBeginPath(this, args);
				}
			}
			else
			{
				if (OnContinuePath != null)
				{
					UnitContinuePathEventArgs args = new UnitContinuePathEventArgs
					{
						Unit = this,
						Path = RemainingRoute
					};

					OnContinuePath(this, args);
				}
			}
		}



		/// <summary>
		/// Engine event. Notifies the unit that it's ready for new actions.
		/// The EndMove event will be fired if the Location is the same as the Destination.
		/// </summary>
        public void Ready(ActionBase action)
        {
            //call the EndMove first 
            if (action.ActionType == ActionType.Move)
            {
                EndMove((action as MoveAction).Location);
            }

            ReadyInternal(action);
        }

		#endregion engine events

        #region command methods

        /// <summary>
        /// Attack order.
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
		public bool AttackUnitOrder(CharacterBase unit)
		{
			throw new NotImplementedException();

			//first thing we must check that the unit can be attacked

			unit.OnDie += AttackedUnitDiedOrOutOfRange;
			unit.OnEndMove += AttackedUnitMoved;
			if (!IsInAttackRange(unit))
			{
				if (!MoveToAttack(unit)) return false;
			}
			else
			{

			}
		}


		/// <summary>
		/// Move order.
		/// </summary>
		public bool MoveOrder(Location targetLocation)
		{
            return Move(targetLocation);
		}

        #endregion command methods

		#region protected methods

        public bool Move(Location targetLocation)
        {
            List<Location> calculatedRoute = FindPath(targetLocation);
            if (calculatedRoute == null) return false;

            if (Destination.HasValue)
            {
                Location oldDestination = Destination.Value;
            }

            RemainingRoute = calculatedRoute;
            RouteTotalLegsCount = calculatedRoute.Count;
            Destination = targetLocation;

            if (!Engine.ProcessMove(this, RemainingRoute[0]))
            {
                return false;
            }

            return true;
        }

        protected virtual void ReadyInternal(ActionBase finishedAction)
		{

		}
		
		/// <summary>
		/// take a nap, up to a 1000 ticks - 10,000 msec
		/// </summary>
		protected void Idle()
		{
			Engine.ProcessIdle(this, Math.Max(50, Random.Next(1000)));
		}

		/// <summary>
		/// Moves to the next leg (location) in the current route.
		/// </summary>
		/// <returns>true - if command accepted by the engine, false - if the next location is not accessible.</returns>
		protected bool MoveToNextLegInTheRoute()
		{
			RemainingRoute.RemoveAt(0);

			Location nextLeg = RemainingRoute[0];

			return Engine.ProcessMove(this, nextLeg);
		}

        /// <summary>
        /// Sets the new location and notifies the UI.
        /// </summary>
        /// <param name="location">The new location.</param>
        protected void EndMove(Location location)
        {
            Location lastLocation = Location;
            Location = location;
            Logging.LogUnit(Id, string.Format("(x:{0}, y:{1})", location.X, location.Y));


            if (OnEndMove != null)
            {
                UnitEndMoveEventArgs args = new UnitEndMoveEventArgs
                {
                    Unit = this,
                    LastLocation = lastLocation,
                    NewLocation = location
                };

                OnEndMove(this, args);
            }

            //if we arrived at the destination then raise the OnEndPath event
            if (Location == Destination)
            {
                RouteEnded(false);
            }
        }

		/// <summary>
		/// Resets the Destination and RemainingRoute. Sends the OnEndPath event.
		/// </summary>
		/// <param name="interrupted">false - if the destination has been reached, true - otherwise.</param>
		protected void RouteEnded(bool interrupted)
		{
			Destination = null;
			RemainingRoute = null;

			Logging.LogUnit(Id, string.Format("Path interrupted at (x:{0}, y:{1})", Location.X, Location.Y));

			if (OnEndPath != null)
			{
				UnitEndPathEventArgs args = new UnitEndPathEventArgs
				{
					Unit = this,
					Location = Location,
					//Destination is set to null in EndMove if it is reached
					PathInterrupted = interrupted
				};
				OnEndPath(this, args);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="targetLocation"></param>
		/// <returns>null - if the path could not be found; a list of Locations that represent all the legs 
		/// to the targetLocation, excluding the current position.
		/// </returns>
		protected List<Location> FindPath(Location targetLocation)
		{
			int[,] mapZone = Map.MapArray;
			byte[,] byteMap = new byte[Map.MapWidth, Map.MapHeight];
			for (int x = 0; x < Map.MapWidth; x++)
			{
				for (int y = 0; y < Map.MapHeight; y++)
				{
					if (mapZone[x, y] == 0)
					{
						byteMap[x, y] = 1;
					}
					else
					{
						byteMap[x, y] = 255;
					}
				}
			}

			PathFinder pathFinder = new PathFinder(byteMap) {Diagonals = false};

			Point start = new Point(Location.X, Location.Y);
			Point end = new Point(targetLocation.X, targetLocation.Y);

			List<PathFinderNode> nodes = pathFinder.FindPath(start, end);

			if (nodes == null) return null;

			//first node is the current position so we remove it.
			nodes.RemoveAt(0);

			List<Location> path = nodes.Select(x => new Location { X = x.X, Y = x.Y }).ToList();

			return path;
		}

		protected bool MoveToARandomPosition()
		{
			int x = Random.Next(Map.MapWidth);
			int y = Random.Next(Map.MapHeight);

			Location location = new Location { X = x, Y = y, Z = 0 };

			if (!Map.IsEmptyCell(location)) return false;

			//if could not move, try again later
			if (Move(location)) return true;

			return false;
		}

		//TODO: delete or rename the method
		protected bool CanMoveTo(Location location)
		{
			//TODO: need a more advanced logic here

			return Map.IsEmptyCell(location);
		}

		protected virtual void AttackedUnitMoved(object sender, UnitEventArgs unitArgs)
		{
			if (!IsInRange(unitArgs.Unit))
			{
				AttackedUnitDiedOrOutOfRange(sender, unitArgs);
			}
			else
			{
				MoveToAttack(unitArgs.Unit);
			}
		}

		protected virtual bool MoveToAttack(CharacterBase unit)
		{
			throw new NotImplementedException();
		}

		protected virtual void AttackedUnitDiedOrOutOfRange(object sender, UnitEventArgs unitArgs)
		{
			//stop chase
		}

		/// <summary>
		/// Checks if the unit is in sensory range.
		/// </summary>
		protected virtual bool IsInRange(CharacterBase unit)
		{
			return CartesianDistance(unit.Location) <= Range;
		}

		protected virtual bool IsInAttackRange(CharacterBase unit)
		{
			//TODO: in the future should check if the unit uses a ranged weapon and 
			//do a Cartesian distance calculation

			return DistanceToUnit(unit) <= AttackRange;
		}

		/// <summary>
		/// Returns a cell distance to the unit not considering any obstacles.
		/// </summary>
		protected virtual int DistanceToUnit(CharacterBase unit)
		{
			return Math.Abs(Location.X - unit.Location.X) + Math.Abs(Location.Y - unit.Location.Y);
		}

		protected int CartesianDistance(Location location)
		{
			return (int)Math.Sqrt(Math.Sqrt(Location.X - location.X) + (Location.Y - location.Y));
		}

		#endregion protected
	}
}
