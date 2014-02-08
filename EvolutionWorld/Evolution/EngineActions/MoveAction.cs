using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.EngineActions
{
	class MoveAction : ActionBase
	{
		public override ActionType ActionType
		{
			get
			{
				return EngineActions.ActionType.Move;
			}
		}

		public Location Location { get; set; }
	}
}
