﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Events
{
	public class UnitContinuePathEventArgs : UnitEventArgs
	{
		public List<Location> Path { get; set; }
	}
}
