using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Events
{
    public class UnitEventArgs : EventArgs
    {
        public UnitBase Unit { get; set; }
    }
}
