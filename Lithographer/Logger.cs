#region License

/* Copyright (c) 2023 darkerbit
 *
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 *
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 *
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would be
 *    appreciated but is not required.
 * 2. Altered source versions must be plainly marked as such, and must not be
 *    misrepresented as being the original software.
 * 3. This notice may not be removed or altered from any source distribution.
 */

#endregion

#region Using statements

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

#endregion

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
		public static string LastLine { get; set; }

		private static readonly Queue<string> lines = new Queue<string>();
		private static Task loggerTask;
		private static readonly ManualResetEvent newLine = new ManualResetEvent(false);
		private static readonly ManualResetEvent terminate = new ManualResetEvent(false);

		private static void Log(Line line)
		{
			string plainLine = $"[{line.Prefix}] ({line.Severity}) {line.Message}";

			lock (lines)
			{
				lines.Enqueue(plainLine);
				newLine.Set();
			}

			if (line.Severity >= Severity.Warning)
			{
				System.Console.Error.WriteLine(plainLine);
			}
			else
			{
				System.Console.WriteLine(plainLine);
			}

			Debug.WriteLine(plainLine);

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

		public static void StartFileLogger(string path)
		{
			loggerTask = Task.Run(() =>
			{
				using (StreamWriter writer = new StreamWriter(File.Open(
					Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path), FileMode.Create
				)))
				{
					while (true)
					{
						if (WaitHandle.WaitAny(new WaitHandle[] { newLine, terminate }) == 1)
						{
							return;
						}

						newLine.Reset();

						Queue<string> linesCopy;

						lock (lines)
						{
							linesCopy = new Queue<string>(lines);
							lines.Clear();
						}

						while (linesCopy.Count > 0)
						{
							writer.WriteLine(linesCopy.Dequeue());
						}
					}
				}
			});
		}

		public static void Shutdown()
		{
			terminate.Set();
			loggerTask.Wait();
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
