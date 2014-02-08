using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.Characters;

namespace Evolution.Events
{
    public class UnitEventArgs : EventArgs
    {
        public UnitBase Unit { get; set; }
    }
}
