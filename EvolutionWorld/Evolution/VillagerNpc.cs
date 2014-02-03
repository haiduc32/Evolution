using Evolution.EngineActions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution
{
    public class VillagerNpc : UnitBase
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

		public override void Ready(ActionType finishedAction)
		{
			//call the base first!
			base.Ready(finishedAction);

			if (true)//!IsUnderAttack() && !IsAttacking() && !IsEnRoute())
			{
				if (finishedAction == ActionType.Idle)
				{
					Debug.WriteLineIf(Id == 0, "VillagerNpc: Finished idling.");
					
					//move somewhere
					Debug.WriteLineIf(Id == 0, "VillagerNpc:  Moving to random position.");
					if (!MoveToARandomPosition())
					{
						//if it was impossible to move to a random location then idle
						Debug.WriteLineIf(Id == 0, "VillagerNpc: Idling because can't go to a random position.");
						Idle();
					}
					
				}
				else if (finishedAction == ActionType.Move)
				{
					//check that the unit arrived at the destination
					if (Destination.HasValue && Location != Destination)
					{
						Debug.WriteLineIf(Id == 0, "VillagerNpc: Did not arrive at the destination.");
						//something blocked the path
						//try to get another route

						if (!CanMoveTo(Destination.Value))
						{
							Debug.WriteLineIf(Id == 0, "VillagerNpc: Destination obstructed. Moving to a new random position.");
							if (!MoveToARandomPosition())
							{
								//if it was impossible to move to a random location then idle
								Debug.WriteLineIf(Id == 0, "VillagerNpc: Idling because can't go to a random position.");
								Idle();
							}
						}
						else
						{
							if (!Move(Destination.Value))
							{
								Debug.WriteLineIf(Id == 0, "VillagerNpc: Path obstructed. Idling.");
								//moving proved to be impossible, so we'll idle for a while
								Idle();
							}
						}
					}
					else
					{
						Debug.WriteLineIf(Id == 0, "VillagerNpc: Idling after a journey.");
						//take a nap, up to a 1000 ticks - 10,000 msec
						Idle();
					}
				}
			}
		}

		private void Idle()
		{
			Engine.ProcessIdle(this, Random.Next(1000));
		}

		private bool MoveToARandomPosition()
		{
			//while (true)
			//{
				//random position
				int x = Random.Next(Map.MapWidth);
				int y = Random.Next(Map.MapHeight);

				//if (Id == 0)
				//{
				//	x = 6;
				//	y = 1;
				//}
				//else
				//{
				//	x = 5;
				//	y = 0;
				//}

				Location location = new Location { X = x, Y = y, Z = 0};
				if (CanMoveTo(location))
				{
					//if could not move, try again later
					if (!Move(location)) return false;


					//break;
				}
				else return false;
			//}

			return true;
		}
	}
}
