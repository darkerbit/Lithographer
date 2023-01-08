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

using ImGuiNET;

using Microsoft.Xna.Framework;

using Vector2 = System.Numerics.Vector2;
using Vector4 = System.Numerics.Vector4;

#endregion

namespace Lithographer
{
	public class LithographerGame : Game
	{
		[STAThread]
		internal static void Main()
		{
			Logger fnaLogger = new Logger("FNA");

			FNALoggerEXT.LogError = fnaLogger.LogError;
			FNALoggerEXT.LogInfo = fnaLogger.Log;
			FNALoggerEXT.LogWarn = fnaLogger.LogWarn;

			Logger.StartFileLogger("log.txt");

			using (LithographerGame game = new LithographerGame())
			{
				game.Run();
			}

			Logger.Shutdown();
		}

		private static readonly Logger logger = new Logger("Lithographer");

		private ImGuiRenderer imRenderer;

		private readonly string buttonString;

		private static readonly string[] buttonStrings =
		{
			"Morb", "Open Celeste", "Download more RAM", "Overthrow the government", "Open FL Studio", "Open aseprite",
			"Open OMORI", "Install Linux", "Re-install Windows", "Subscribe to twitch.tv/shayy", "Summon Sumire",
			"Download Windows 11",
		};

		private bool autoScroll = true;

		private LithographerGame()
		{
			GraphicsDeviceManager graphics = new GraphicsDeviceManager(this);
			graphics.PreferredBackBufferWidth = 1024;
			graphics.PreferredBackBufferHeight = 768;

			IsMouseVisible = true;
			IsFixedTimeStep = false;

			buttonString = buttonStrings[new Random().Next(buttonStrings.Length)];
		}

		protected override void LoadContent()
		{
			imRenderer = new ImGuiRenderer(this);
			imRenderer.RebuildFontAtlas();

			base.LoadContent();
		}

		private bool about;

		private bool imguiDemo;
		private bool imguiInfo;

		// need to do this because of id stack problems
		private bool openPopup;

		protected override void Draw(GameTime gameTime)
		{
			try
			{
				GraphicsDevice.Clear(Color.CornflowerBlue);
				imRenderer.BeforeLayout(gameTime);

				if (ImGui.BeginMainMenuBar())
				{
					if (ImGui.BeginMenu("About"))
					{
						ImGui.MenuItem("About Lithographer", null, ref about);

						ImGui.Separator();

						ImGui.MenuItem("ImGui Demo Window", null, ref imguiDemo);
						ImGui.MenuItem("About Dear ImGui", null, ref imguiInfo);

						ImGui.EndMenu();
					}

					ImGui.EndMainMenuBar();
				}

				if (about)
				{
					AboutWindow();
				}

				if (imguiDemo)
				{
					ImGui.ShowDemoWindow(ref imguiDemo);
				}

				if (imguiInfo)
				{
					ImGui.ShowAboutWindow(ref imguiInfo);
				}

				MainWindow();

				if (openPopup)
				{
					ImGui.OpenPopup("Open file...");
					openPopup = false;
				}

				dialog?.Update();

				base.Draw(gameTime);

				imRenderer.AfterLayout();
			}
			catch (Exception e)
			{
				logger.LogError(e.Message);

				if (e.StackTrace != null)
				{
					logger.LogError(e.StackTrace);
				}
			}
		}

		private void AboutWindow()
		{
			if (ImGui.Begin("About Lithographer", ref about, ImGuiWindowFlags.AlwaysAutoResize))
			{
				ImGui.Text("Lithographer");
				ImGui.Text("Created by darkerbit");

				ImGui.Separator();

				ImGui.Text("Tools used:");
				ImGui.Text("- .NET Framework 4.6.2");
				ImGui.Text("- Mono");
				ImGui.Text("- JetBrains Rider");
				ImGui.Selectable("- Dear ImGui", ref imguiInfo);
				ImGui.Text("- FNA");
				ImGui.Text("- ffmpeg");
			}

			ImGui.End();
		}

		private ImGuiFileDialog dialog;

		private string inputImage = "";
		private string inputMusic = "";

		private string output = "";

		private void MainWindow()
		{
			if (ImGui.Begin("Lithographer", ImGuiWindowFlags.AlwaysAutoResize))
			{
				ImGui.InputText("Image", ref inputImage, 1024);
				ImGui.SameLine();

				if (ImGui.Button("Open...##image"))
				{
					dialog = new ImGuiFileDialog(inputImage, path => inputImage = path);
					openPopup = true;
				}

				ImGui.InputText("Music", ref inputMusic, 1024);
				ImGui.SameLine();

				if (ImGui.Button("Open...##music"))
				{
					dialog = new ImGuiFileDialog(inputMusic, path => inputMusic = path);
					openPopup = true;
				}

				ImGui.Separator();

				ImGui.InputText("Output", ref output, 1024);
				ImGui.SameLine();

				if (ImGui.Button("Save..."))
				{
					dialog = new ImGuiFileDialog(output, path => output = path);
					openPopup = true;
				}

				ImGui.Separator();

				bool disabled = FfmpegRunner.Running ||
					String.IsNullOrWhiteSpace(inputImage) ||
					String.IsNullOrWhiteSpace(inputMusic) ||
					String.IsNullOrWhiteSpace(output);

				ImGui.BeginDisabled(disabled);

				if (ImGui.Button(buttonString) && !disabled)
				{
					FfmpegRunner.Run(inputImage, inputMusic, output);
				}

				ImGui.EndDisabled();

				ImGui.Separator();

				lock (Logger.Console)
				{
					ImGui.TextDisabled("Console");

					ImGui.Checkbox("Auto-Scroll", ref autoScroll);

					if (ImGui.BeginChild("Console", new Vector2(800, 400), true, ImGuiWindowFlags.HorizontalScrollbar))
					{
						for (int i = Logger.Start; i != Logger.End; i = (i + 1) % Logger.LOG_SIZE)
						{
							Logger.Line line = Logger.Console[i];

							ImGui.TextDisabled(line.Prefix);
							ImGui.SameLine();

							switch (line.Severity)
							{
								case Logger.Severity.Error:
									ImGui.TextColored(new Vector4(1, 0, 0, 1), line.Message);
									break;

								case Logger.Severity.Warning:
									ImGui.TextColored(new Vector4(1, 1, 0, 1), line.Message);
									break;

								case Logger.Severity.Info:
								default:
									ImGui.TextUnformatted(line.Message);
									break;
							}

							if (ReferenceEquals(line.Message, Logger.LastLine) && autoScroll)
							{
								ImGui.SetScrollHereY();

								Logger.LastLine = null;
							}
						}
					}

					ImGui.EndChild();
				}
			}

			ImGui.End();
		}
	}
}
