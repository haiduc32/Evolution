using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.EngineActions
{
	class IdleAction : ActionBase
	{
		public override ActionType ActionType
		{
			get { return EngineActions.ActionType.Idle; }
		}
	}
}
