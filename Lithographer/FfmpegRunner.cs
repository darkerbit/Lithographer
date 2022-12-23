using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using SDL2;

namespace Lithographer
{
	public static class FfmpegRunner
	{
		public static bool Running { get; private set; }

		private static readonly Logger FfmpegLogger = new Logger("ffmpeg");

		public static void Run(string image, string music, string outFile)
		{
			Task.Run(() =>
			{
				string os = SDL.SDL_GetPlatform();

				Running = true;

				Process process = new Process
				{
					StartInfo = new ProcessStartInfo
					{
						FileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
							os.Equals("Windows") ? "ffmpeg.exe" : "ffmpeg"),
						Arguments =
							$"-y -loglevel warning -stats -loop 1 -i \"{image}\" -i \"{music}\" -shortest -acodec aac -b:a 320k -vcodec h264 -preset veryslow -tune stillimage -pix_fmt yuv420p \"{outFile}\"",
						ErrorDialog = true,
						CreateNoWindow = true,
						UseShellExecute = false,
						RedirectStandardError = true,
						RedirectStandardOutput = false,
					},
				};

				process.Start();

				while (!process.HasExited)
				{
					string line = process.StandardError.ReadLine();

					if (line != null)
					{
						FfmpegLogger.Log(line);
					}
				}

				process.WaitForExit();
				process.Close();
				Running = false;

				FfmpegLogger.Log("Done!");
			});
		}
	}
}
