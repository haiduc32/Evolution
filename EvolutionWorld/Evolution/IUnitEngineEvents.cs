using Evolution.Characters;
using Evolution.EngineActions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution
{
	interface IUnitEngineEvents
	{
		void UnitInRange(UnitBase unit);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="location"></param>
		/// <returns>true - if the path it is on should be automatically continued by the engine; false - the path (if any) should be stopped</returns>
		void EndMove(Location location);

		void UnitOutOfRange(UnitBase unit);

		void Ready(ActionType finishedAction);
	}
}
