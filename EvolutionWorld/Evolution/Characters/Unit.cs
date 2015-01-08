using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Characters
{
    public abstract class Unit
    {
        public Location Location { get; protected set; }
    }
}
