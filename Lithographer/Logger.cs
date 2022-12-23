using System.Diagnostics;

namespace Lithographer
{
	public class Logger
	{
		public static string[] Console { get; } = new string[1024];
		public static int Start { get; private set; }
		public static int End { get; private set; }
		public static string LastLine { get; private set; }

		public static void Log(string prefix, string msg)
		{
			string formatted = $"[{prefix.Trim()}] {msg.Trim()}";

			System.Console.WriteLine(formatted);
			Debug.WriteLine(formatted);
			
			LogConsole(formatted);
		}

		private static void LogConsole(string line)
		{
			lock (Console)
			{
				LastLine = line;

				Console[End++] = line;

				if (End >= Console.Length)
				{
					End = 0;
				}

				if (End == Start)
				{
					Start++;
				}

				if (Start >= Console.Length)
				{
					Start = 0;
				}
			}
		}
		
		public string Prefix { get; }

		public Logger(string prefix)
		{
			Prefix = prefix;
		}

		public void Log(string msg)
		{
			Log(Prefix, msg);
		}
	}
}
