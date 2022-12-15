using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Lithographer
{
	public static class FfmpegRunner
	{
		public static bool Running { get; private set; }

		public static void Run(string image, string music, string outFile)
		{
			Task.Run(() =>
			{
				Running = true;

				var process = new Process
				{
					StartInfo = new ProcessStartInfo
					{
						FileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
							RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "ffmpeg.exe" : "ffmpeg"),
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
					var line = process.StandardError.ReadLine();
					
					if (line != null)
						LithographerGame.Log(line);
				}

				process.WaitForExit();
				process.Close();
				Running = false;

				LithographerGame.Log("Done!");
			});
		}
	}
}
