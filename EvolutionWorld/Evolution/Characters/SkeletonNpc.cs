using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.EngineActions;

namespace Evolution.Characters
{
	public class SkeletonNpc : UnitBase
	{
		#region default properties

		public override int Range
		{
			get { return 3; }
		}

		public override bool IsNpc
		{
			get { return true; }
		}

		public override int MovementSpeed
		{
			get { return 50; }
		}

		#endregion default properties

		public override void Ready(ActionType finishedAction)
		{
			//call the base first!
			base.Ready(finishedAction);

			bool actionTaken = false;

			//if attacking a unit
			if (AttackedUnit != null)
			{
				
			}
			//if there is an unit worth attacking in range, attack it
			//else if (UnitsInRange)

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
								PathEnded(true);
							}
						}
					}
				}
			}

			//if there was no action then just idle
			if (!actionTaken)
			{
				Idle();
			}
		}

		public override void UnitInRange(UnitBase unit)
		{
			base.UnitInRange(unit);

			//if is attacking, then ignore any new units
			if (AttackedUnit != null) return;

			//if the new unit in range is a player or a villager npc, stop idling (supposedly)
			//and attack it in the Ready() method
			if (!unit.IsNpc || unit is VillagerNpc)
			{
				Engine.CancelAllActions(this);
			}
		}


	}
}
