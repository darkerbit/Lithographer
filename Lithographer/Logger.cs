using System.Diagnostics;

namespace Lithographer
{
	public class Logger
	{
		public enum Severity
		{
			Info,
			Warning,
			Error,
		}

		public readonly struct Line
		{
			public string Prefix { get; }
			public Severity Severity { get; }
			public string Message { get; }

			internal Line(string prefix, Severity severity, string message)
			{
				Prefix = prefix;
				Severity = severity;
				Message = message;
			}
		}
		
		public const int LOG_SIZE = 1024;
		
		public static Line[] Console { get; } = new Line[LOG_SIZE];
		public static int Start { get; private set; }
		public static int End { get; private set; }
		public static string LastLine { get; private set; }

		private static void Log(Line line)
		{
			string console = $"[{line.Prefix}] ({line.Severity}) {line.Message}";

			if (line.Severity >= Severity.Warning)
			{
				System.Console.Error.WriteLine(console);
			}
			else
			{
				System.Console.WriteLine(console);
			}
			
			Debug.WriteLine(console);
			
			LogConsole(line);
		}

		private static void LogConsole(Line line)
		{
			lock (Console)
			{
				LastLine = line.Message;

				Console[End++] = line;

				if (End >= LOG_SIZE)
				{
					End = 0;
				}

				if (End == Start)
				{
					Start++;
				}

				if (Start >= LOG_SIZE)
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
			Log(new Line(Prefix, Severity.Info, msg));
		}

		public void LogWarn(string msg)
		{
			Log(new Line(Prefix, Severity.Warning, msg));
		}
		
		public void LogError(string msg)
		{
			Log(new Line(Prefix, Severity.Error, msg));
		}
	}
}
