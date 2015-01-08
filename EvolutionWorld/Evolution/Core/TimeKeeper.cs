using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Evolution.Core
{
	class TimeKeeper
	{
        /// <summary>
        /// Interval for ticks in milliseconds
        /// </summary>
		private const long TickTime = 10;

		public delegate void TickDelegate(long tick);

		private Stopwatch _stopwatch;

		private bool _stopSignal;

		private long _tick;
		private long _lastMSec = 0;
		private long _tickInterval = TickTime;

		public TickDelegate OnTick { get; set; }

		public long Tick { get { return _tick; } }

		public TimeKeeper()
		{
			_stopwatch = new Stopwatch();
		}

		public void Start()
		{
			_stopSignal = false;

			Task clockTask = new Task(() =>
			{
				try
				{
					List<long> tickTimes = new List<long>();

					_stopwatch.Start();
					while (true)
					{
						//check if time has incremented with 10msec, and then move to processing the pending actions
						int sleepTime = (int)(_tickInterval < 0 ? 0 : _tickInterval);

						if (_stopSignal) return;

						Thread.Sleep((int)sleepTime);

						if (_stopSignal) return;
						
						long timeElapsed = _stopwatch.ElapsedMilliseconds;
						long slept = timeElapsed - _lastMSec;
						_lastMSec = timeElapsed;

						long deltaCorrection = slept - (TickTime - (TickTime - _tickInterval));
						_tickInterval = TickTime - deltaCorrection;

						_tick++;
						
						if (OnTick != null)
						{
							OnTick(_tick);
						}
					}
				}
				finally
				{
					_stopwatch.Stop();
				}
			});

			clockTask.Start();
		}

		public void Stop()
		{
			_stopSignal = true;
		}
	}
}
