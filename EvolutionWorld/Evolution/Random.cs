using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution
{
	public static class Random
	{
		private static System.Random _random;

		static Random()
		{
			_random = new System.Random();
		}

		public static int Next(int max)
		{
			return _random.Next(max);
		}
	}
}
