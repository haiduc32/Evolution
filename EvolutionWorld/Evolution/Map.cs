using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Evolution
{
	/// <summary>
	/// MapBlock - a square cluster of cells.
	/// MapCluster - a cluster of blocks.
	/// </summary>
	public class Map
	{
		private const int MAP_HEIGHT = 16;
		private const int MAP_WIDTH = 16;

		/// <summary>
		/// keeps track of all the stuff on the map.
		/// </summary>
		private int[,] _map;

		private MapIdentifierPool _identifierPool;

		private Dictionary<object, MapIdentifier> _objectIdDictionary;

		//TODO: shouldn't be available in this form..
		public int[,] MapArray { get { return _map; } }

		public int MapHeight { get { return MAP_HEIGHT; } }

		public int MapWidth { get { return MAP_WIDTH; } }

		public Map()
		{
			_identifierPool = new MapIdentifierPool(1024);
			_map = new int[MAP_WIDTH, MAP_HEIGHT];
			_objectIdDictionary = new Dictionary<object, MapIdentifier>();
		}

		public void Add(UnitBase unit, Location location)
		{
			MapIdentifier identifier = _identifierPool.Get();
			_objectIdDictionary.Add(unit, identifier);
			_map[location.X, location.Y] = identifier.Id;
		}

		public void Move(UnitBase unit, Location oldLocation, Location newLocation)
		{
			_map[oldLocation.X, oldLocation.Y] = 0;
			_map[newLocation.X, newLocation.Y] = _objectIdDictionary[unit].Id;
		}

		public void Remove(UnitBase unit, Location location)
		{
			_map[location.X, location.Y] = 0;
		}

		public bool IsEmptyCell(Location location)
		{
			if (location.X < 0 || location.Y < 0 || location.X > MAP_WIDTH - 1 || location.Y > MAP_HEIGHT - 1) return false;

			return _map[location.X, location.Y] == 0;
		}

		public bool ReserveIfEmpty(Location location, UnitBase reservationOwner)
		{
			//TODO: move to some constants if will keep it (see below)
			const int RESERVED = 1;
			//TODO: maybe set the map id of the unit instead of a reservation code?
			if (_map[location.X, location.Y] == 0 && 0 == Interlocked.CompareExchange(
				ref _map[location.X, location.Y], RESERVED, 0))
			{
				//TODO: track the reservation owner

				return true;
			}

			return false;
		}

		//TODO: should get units by some kind of regions, but for initial testing this will do
		public List<ICommunicationUnit> GetUnits()
		{
			return _objectIdDictionary.Select(x => (ICommunicationUnit)x.Key).ToList();
		}
	}

	public class MapIdentifierPool
	{
		private int _lastGeneratedId;
		private ConcurrentStack<MapIdentifier> _availableStack;
		
		public MapIdentifierPool(int identifierBase)
		{
			_lastGeneratedId = identifierBase;
			_availableStack = new ConcurrentStack<MapIdentifier>();
		}

		public MapIdentifier Get()
		{
			MapIdentifier identifier;
			if (!_availableStack.TryPop(out identifier))
			{
				int newId = Interlocked.Increment(ref _lastGeneratedId);
				identifier = new MapIdentifier(newId);
			}

			return identifier;
		}

		public void Release(MapIdentifier identifier)
		{
			_availableStack.Push(identifier);
		}
	}

	public class MapIdentifier
	{
		public int Id { get; private set; }

		public MapIdentifier(int id)
		{
			Id = id;
		}
	}

	public class MapZone
	{
		public int OriginX { get; set; }

		public int OriginY { get; set; }

	}

	public class MapCluster
	{

	}
}
