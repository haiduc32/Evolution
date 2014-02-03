using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvolutionHost
{
	class TraceConsole
	{
		#region singletone
		private static object _lockObject = new object();
		private static TraceConsole _instance;

		public static TraceConsole Instance
		{
			get
			{
				if (_instance == null)
				{
					lock (_lockObject)
					{
						if (_instance == null)
						{
							_instance = new TraceConsole();
						}
					}

				}

				return _instance;
			}
		}

		#endregion singletone

		private TextWriter _hostConsole;
		private StringWriter _consoleOut;
		/// <summary>
		/// Start tracing console to TraceHub.
		/// </summary>
		public void Start()
		{
			_hostConsole = Console.Out;
			_consoleOut = new StringWriter();
			Console.SetOut(_consoleOut);
			//Console.WriteLine("This is intercepted."); // This is not written to console
			//File.WriteAllText("ConsoleOutput.txt", consoleOut.ToString());

			//_consoleOut.
		}
	}
}
