using Algorithms;
using Evolution.EngineActions;
using Evolution.Events;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution
{
	public abstract class UnitBase : IUnitEngineEvents, ICommunicationUnit
	{
		#region properties

		/// <summary>
		/// Sensory range in number of cells.
		/// </summary>
		public abstract int Range { get; }

		public abstract bool IsNpc { get; }

		/// <summary>
		/// Ticks necessary to move to an adiacent cell. (1/100 of a sec)
		/// </summary>
		public abstract int MovementSpeed { get; }

		public int Life { get; private set; }

		public int MaxLife { get; private set; }

		public Location Location { get; private set; }

		public int ExperiencePoints { get; private set; }

		public UnitBase AttackedUnit { get; private set; }

		public bool IsMoving { get; private set; }

		public Location? Destination { get; private set; }

		public List<Location> Route { get; private set; }

		public Engine Engine { get; set; }

		public Map Map { get; set; }

		public int Id { get; set; }

		#endregion properties

		#region events

		public EventHandler<UnitEventArgs> OnDie;

		public EventHandler<UnitBeginMoveEventArgs> OnBeginMove;

		public EventHandler<UnitEndMoveEventArgs> OnEndMove;

		public EventHandler<UnitBeginPathEventArgs> OnBeginPath;

		public EventHandler<UnitContinuePathEventArgs> OnContinuePath;

		public EventHandler<UnitEndPathEventArgs> OnEndPath;

		#endregion events

		#region engine events

		/// <summary>
		/// Engine event. Called when a new unit comes in range.
		/// </summary>
		public virtual void UnitInRange(UnitBase unit)
		{
		}

		/// <summary>
		/// Engine event. Called when a unit gets out of range (or also if dies (confirm?)).
		/// </summary>
		public virtual void UnitOutOfRange(UnitBase unit)
		{

		}

		/// <summary>
		/// Engine event. Notifies the unit that it's starting to move.
		/// </summary>
		/// <param name="targetLocation">Can be only an adiacent cell.</param>
		/// <param name="pathLegsLeft">The count of path legs left including the current move.</param>
		public virtual void BeginMove(Location targetLocation, int pathLegsLeft)
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

			if (pathLegsLeft == Route.Count)
			{
				if (OnBeginPath != null)
				{
					UnitBeginPathEventArgs args = new UnitBeginPathEventArgs
					{
						Unit = this,
						Path = this.Route
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
						Path = this.Route.Skip(this.Route.Count - pathLegsLeft).ToList()
					};

					OnContinuePath(this, args);
				}
			}
		}

		/// <summary>
		/// Engine event. Notifies the unit that it's position has changed.
		/// </summary>
		public virtual void EndMove(Location location)
		{
			Location lastLocation = this.Location;
			this.Location = location;

			if (location == Destination)
			{
				Destination = null;
				Route = null;
			}

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
		}

		/// <summary>
		/// Engine event. Notifies the unit that it's ready for new actions.
		/// </summary>
		public virtual void Ready(ActionType finishedAction)
		{
			if (finishedAction == ActionType.Move)
			{
				if (OnEndPath != null)
				{
					UnitEndPathEventArgs args = new UnitEndPathEventArgs
					{
						Unit = this,
						Location = Location,
						//Destination is set to null in EndMove if it is reached
						PathInterrupted = Destination != null
					};
					OnEndPath(this, args);
				}
			}
		}

		#endregion engine events

		public bool AttackUnit(UnitBase unit)
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
		/// Command.
		/// </summary>
		public bool Move(Location targetLocation)
		{
			//TODO: should return false if moving to the position is not possible

			List<Location> calculatedRoute = FindPath(targetLocation);
			if (calculatedRoute == null) return false;

			Route = calculatedRoute;
			Destination = targetLocation;

			//List<Location> path = new List<Evolution.Location> { targetLocation };
			if (!Engine.ProcessMove(this, calculatedRoute))
			{
				return false;
			}

			return true;
		}

		#region protected methods

		/// <summary>
		/// 
		/// </summary>
		/// <param name="targetLocation"></param>
		/// <returns>null - if the path could not be found</returns>
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

			PathFinder pathFinder = new PathFinder(byteMap);

			pathFinder.Diagonals = false;

			Point start = new Point(this.Location.X, this.Location.Y);
			Point end = new Point(targetLocation.X, targetLocation.Y);

			List<PathFinderNode> nodes = pathFinder.FindPath(start, end);

			if (nodes == null) return null;

			//first node is the current position so we remove it.
			nodes.RemoveAt(0);

			List<Location> path = nodes.Select(x => new Location { X = x.X, Y = x.Y }).ToList();

			return path;
		}

		protected void CheckConcerns()
		{
			//checks what it should do now
			//TODO: not sure if it's relevant
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

		protected virtual bool MoveToAttack(UnitBase unit)
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
		/// <param name="unit"></param>
		/// <returns></returns>
		protected virtual bool IsInRange(UnitBase unit)
		{
			throw new NotImplementedException();
		}

		protected virtual bool IsInAttackRange(UnitBase unit)
		{
			//checks if a unit is still in range

			//so far nobody supports attacking from different hight levels
			if (unit.Location.Z != this.Location.Z) return false;

			if (Math.Abs(unit.Location.X - this.Location.X) > 1) return false;

			if (Math.Abs(unit.Location.Y - this.Location.Y) > 1) return false;

			return true;
		}

		#endregion protected
	}
}
