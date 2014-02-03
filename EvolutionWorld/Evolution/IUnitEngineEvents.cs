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

		void EndMove(Location location);

		void UnitOutOfRange(UnitBase unit);

		void Ready(ActionType finishedAction);
	}
}
