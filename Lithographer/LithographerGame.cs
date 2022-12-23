using System;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Vector2 = System.Numerics.Vector2;
using Vector4 = System.Numerics.Vector4;

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

		private static readonly Logger LithoLogger = new Logger("Lithographer");

		private ImGuiRenderer _imRenderer;

		private readonly string _buttonString;

		private static readonly string[] ButtonStrings =
		{
			"Morb", "Open Celeste", "Download more RAM", "Overthrow the government", "Open FL Studio", "Open aseprite",
			"Open OMORI", "Install Linux", "Re-install Windows", "Subscribe to twitch.tv/shayy", "Summon Sumire",
			"Download Windows 11",
		};

		private bool _autoScroll = true;

		private LithographerGame()
		{
			GraphicsDeviceManager graphics = new GraphicsDeviceManager(this);
			graphics.PreferredBackBufferWidth = 1024;
			graphics.PreferredBackBufferHeight = 768;

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

				base.Draw(gameTime);

				_imRenderer.AfterLayout();
			}
			catch (Exception e)
			{
				LithoLogger.LogError(e.Message);

				if (e.StackTrace != null)
				{
					LithoLogger.LogError(e.StackTrace);
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

				lock (Logger.Console)
				{
					ImGui.TextDisabled("Console");

					ImGui.Checkbox("Auto-Scroll", ref _autoScroll);

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

							if (line.Message == Logger.LastLine && _autoScroll)
							{
								ImGui.SetScrollHereY();
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
