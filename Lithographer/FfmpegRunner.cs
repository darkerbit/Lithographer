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
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

using SDL2;

#endregion

namespace Lithographer
{
	public static class FfmpegRunner
	{
		public static bool Running { get; private set; }

		private static readonly Logger logger = new Logger("ffmpeg");

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

					if (!String.IsNullOrWhiteSpace(line))
					{
						logger.Log(line);
					}
				}

				process.WaitForExit();
				process.Close();
				Running = false;

				logger.Log("Done!");
			});
		}
	}
}
