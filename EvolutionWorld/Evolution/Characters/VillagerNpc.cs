using Evolution.EngineActions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Characters
{
    public class VillagerNpc : CharacterBase
    {
		public override int Range
		{
			get { return 4; }
		}

		public override bool IsNpc
		{
			get { return true; }
		}

		public override int MovementSpeed
		{
			get { return 50; }
		}

		protected override void ReadyInternal(ActionBase action)
		{
			bool actionTaken = false;

            ActionType finishedAction = action.ActionType;

			//call the base for actions that we don't handle
            if (finishedAction != ActionType.Idle && finishedAction != ActionType.Move)
            {
                base.ReadyInternal(action);
                return;
            }

			if (true)//!IsUnderAttack() && !IsAttacking() && !IsEnRoute())
			{
				if (finishedAction == ActionType.Idle)
				{
					Debug.WriteLineIf(Id == 0, "VillagerNpc: Finished idling.");
					
					//move somewhere
					Debug.WriteLineIf(Id == 0, "VillagerNpc:  Moving to random position.");
					actionTaken = MoveToARandomPosition();
					
				}
				else if (finishedAction == ActionType.Move)
				{
					// if the unit did not arrive at the destination, then we need to move to the next leg
					if (IsFollowingRoute)
					{
						actionTaken = MoveToNextLegInTheRoute();

						if (!actionTaken)
						{
							//the next leg is obstructed, try to find a bypass
							actionTaken = Move(Destination.Value);
							//if moving to the initial destination is not possible
							//we should let the unit to idle till it's possible to move somewhere 
							//or ther actions are possible

							if (!actionTaken)
							{
								RouteEnded(true);
							}
						}
					}
				}
			}

			//if there was no action then just idle
			if (!actionTaken)
			{
				Idle();
                actionTaken = true;
			}
		}


	}
}
