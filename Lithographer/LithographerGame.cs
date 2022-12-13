using ImGuiNET;
using Microsoft.Xna.Framework;

namespace Lithographer;

public class LithographerGame : Game
{
	private readonly GraphicsDeviceManager _graphics;

	private ImGuiRenderer _imRenderer;

	public LithographerGame()
	{
		_graphics = new GraphicsDeviceManager(this);
		_graphics.PreferredBackBufferWidth = 640;
		_graphics.PreferredBackBufferHeight = 480;

		Content.RootDirectory = "Content";

		IsMouseVisible = true;
		IsFixedTimeStep = false;
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

	protected override void Draw(GameTime gameTime)
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
			AboutWindow();
		
		if (_imguiDemo)
			ImGui.ShowDemoWindow(ref _imguiDemo);
		
		if (_imguiInfo)
			ImGui.ShowAboutWindow(ref _imguiInfo);
		
		MainWindow();

		_imRenderer.AfterLayout();
		base.Draw(gameTime);
	}

	private void AboutWindow()
	{
		if (ImGui.Begin("About Lithographer", ref _about, ImGuiWindowFlags.AlwaysAutoResize))
		{
			ImGui.Text("Lithographer");
			ImGui.Text("Created by darkerbit");
			
			ImGui.Separator();
			
			ImGui.Text("Tools used:");
			ImGui.Text("- .NET 7");
			ImGui.Text("- JetBrains Rider");
			ImGui.Selectable("- Dear ImGui", ref _imguiInfo);
			ImGui.Text("- FNA");
			ImGui.Text("- ffmpeg");
		}

		ImGui.End();
	}

	private void MainWindow()
	{
		if (ImGui.Begin("Lithographer", ImGuiWindowFlags.AlwaysAutoResize))
		{
		}
		
		ImGui.End();
	}
}
