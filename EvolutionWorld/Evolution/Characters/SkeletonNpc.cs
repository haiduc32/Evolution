using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.EngineActions;

namespace Evolution.Characters
{
	public class SkeletonNpc : CharacterBase
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

		protected override void ReadyInternal(ActionBase action)
		{
            ActionType finishedAction = action.ActionType;
            //TODO: filter by the types of handled actions
			//call the base first!
			base.ReadyInternal(action);

			bool actionTaken = false;

			CharacterBase unitToAttack;

			//if attacking a unit
			if (AttackedUnit != null)
			{
				// if in attack range, just attack it
				// else 
				//      if moving, check that the destination is a cell next to the attacked unit
				//      else adjust the route to the attacked unit
				//      if not moving, then start moving
			}
			//if there is a unit worth attacking in range, attack it
			else if ((unitToAttack = FindUnitToAttack()) != null)
			{
				//let's not forget that if we are moving, we should stop first
				if (IsFollowingRoute)
				{
					RouteEnded(true);
				}

				//TOOD: attack the unit!
				// set the attacked unit property
				AttackedUnit = unitToAttack;
				// if in attack range, just attack it
				Engine.ProcessAttack(this, AttackedUnit);
				// else move to the attacked unit
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
								RouteEnded(interrupted: true);
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

		public override void UnitInRange(CharacterBase unit)
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


		private CharacterBase FindUnitToAttack()
		{
			return UnitsInRange.Where(x => !x.IsNpc || x is VillagerNpc)
				.OrderBy(DistanceToUnit).FirstOrDefault();
		}

	}
}
