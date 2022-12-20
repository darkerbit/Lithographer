using System;
using System.Collections.Generic;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Vector2 = System.Numerics.Vector2;

namespace Lithographer
{
	public class LithographerGame : Game
	{
		[STAThread]
		internal static void Main()
		{
			FNALoggerEXT.LogError += msg => Log($"[FNA ERROR] {msg}");
			FNALoggerEXT.LogInfo += msg => Log($"[FNA] {msg}");
			FNALoggerEXT.LogWarn += msg => Log($"[FNA WARNING] {msg}");
			
			using (LithographerGame game = new LithographerGame())
			{
				game.Run();
			}
		}
		
		private ImGuiRenderer _imRenderer;

		private readonly string _buttonString;

		private static readonly string[] ButtonStrings =
		{
			"Morb", "Open Celeste", "Download more RAM", "Overthrow the government", "Open FL Studio", "Open aseprite",
			"Open OMORI", "Install Linux", "Re-install Windows", "Subscribe to twitch.tv/shayy", "Summon Sumire",
			"Download Windows 11",
		};

		private bool _autoScroll = true;

		private static string _latestLine;

		private static readonly List<string> Console = new List<string>();

		public static void Log(string text)
		{
			lock (Console)
			{
				_latestLine = text;
				Console.Add(text);
			}
		}

		private LithographerGame()
		{
			GraphicsDeviceManager graphics = new GraphicsDeviceManager(this);
			graphics.PreferredBackBufferWidth = 640;
			graphics.PreferredBackBufferHeight = 480;

			IsMouseVisible = true;
			IsFixedTimeStep = false;

			_buttonString = ButtonStrings[new Random().Next(ButtonStrings.Length)];
		}

		protected override void LoadContent()
		{
			_imRenderer = new ImGuiRenderer(this);
			_imRenderer.RebuildFontAtlas();

			base.LoadContent();
		}

		private bool _about;

		private bool _imguiDemo;
		private bool _imguiInfo;

		// need to do this because of id stack problems
		private bool _openPopup;

		protected override void Draw(GameTime gameTime)
		{
			try
			{
				GraphicsDevice.Clear(Color.CornflowerBlue);
				_imRenderer.BeforeLayout(gameTime);

				if (ImGui.BeginMainMenuBar())
				{
					if (ImGui.BeginMenu("About"))
					{
						ImGui.MenuItem("About Lithographer", null, ref _about);

						ImGui.Separator();

						ImGui.MenuItem("ImGui Demo Window", null, ref _imguiDemo);
						ImGui.MenuItem("About Dear ImGui", null, ref _imguiInfo);

						ImGui.EndMenu();
					}

					ImGui.EndMainMenuBar();
				}

				if (_about)
				{
					AboutWindow();
				}

				if (_imguiDemo)
				{
					ImGui.ShowDemoWindow(ref _imguiDemo);
				}

				if (_imguiInfo)
				{
					ImGui.ShowAboutWindow(ref _imguiInfo);
				}

				MainWindow();

				if (_openPopup)
				{
					ImGui.OpenPopup("Open file...");
					_openPopup = false;
				}

				_dialog?.Update();

				_imRenderer.AfterLayout();
				base.Draw(gameTime);
			}
			catch (Exception e)
			{
				Log(e.Message);

				if (e.StackTrace != null)
				{
					Log(e.StackTrace);
				}
			}
		}

		private void AboutWindow()
		{
			if (ImGui.Begin("About Lithographer", ref _about, ImGuiWindowFlags.AlwaysAutoResize))
			{
				ImGui.Text("Lithographer");
				ImGui.Text("Created by darkerbit");

				ImGui.Separator();

				ImGui.Text("Tools used:");
				ImGui.Text("- .NET Framework 4.6.2");
				ImGui.Text("- Mono");
				ImGui.Text("- JetBrains Rider");
				ImGui.Selectable("- Dear ImGui", ref _imguiInfo);
				ImGui.Text("- FNA");
				ImGui.Text("- ffmpeg");
			}

			ImGui.End();
		}

		private ImGuiFileDialog _dialog;

		private string _inputImage = "";
		private string _inputMusic = "";

		private string _output = "";

		private void MainWindow()
		{
			if (ImGui.Begin("Lithographer", ImGuiWindowFlags.AlwaysAutoResize))
			{
				ImGui.InputText("Image", ref _inputImage, 1024);
				ImGui.SameLine();

				if (ImGui.Button("Open...##image"))
				{
					_dialog = new ImGuiFileDialog(_inputImage, path => _inputImage = path);
					_openPopup = true;
				}

				ImGui.InputText("Music", ref _inputMusic, 1024);
				ImGui.SameLine();

				if (ImGui.Button("Open...##music"))
				{
					_dialog = new ImGuiFileDialog(_inputMusic, path => _inputMusic = path);
					_openPopup = true;
				}

				ImGui.Separator();

				ImGui.InputText("Output", ref _output, 1024);
				ImGui.SameLine();

				if (ImGui.Button("Save..."))
				{
					_dialog = new ImGuiFileDialog(_output, path => _output = path);
					_openPopup = true;
				}

				ImGui.Separator();

				bool disabled = FfmpegRunner.Running ||
				                String.IsNullOrWhiteSpace(_inputImage) ||
				                String.IsNullOrWhiteSpace(_inputMusic) ||
				                String.IsNullOrWhiteSpace(_output);

				ImGui.BeginDisabled(disabled);

				if (ImGui.Button(_buttonString) && !disabled)
				{
					FfmpegRunner.Run(_inputImage, _inputMusic, _output);
				}

				ImGui.EndDisabled();

				ImGui.Separator();

				ImGui.TextDisabled("Console");

				ImGui.Checkbox("Auto-Scroll", ref _autoScroll);

				if (ImGui.BeginChild("Console", new Vector2(400, 200), true))
				{
					lock (Console)
					{
						foreach (string line in Console)
						{
							ImGui.TextWrapped(line);

							if (line.Equals(_latestLine) && _autoScroll)
							{
								ImGui.SetScrollHereY();
								_latestLine = null;
							}
						}
					}
				}

				ImGui.EndChild();
			}

			ImGui.End();
		}
	}
}
