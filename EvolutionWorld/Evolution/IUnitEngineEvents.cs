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
        Location Location { get; }

		void UnitInRange(CharacterBase unit);

		void UnitOutOfRange(CharacterBase unit);

		void Ready(ActionBase finishedAction);
	}
}
