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
		_graphics.PreferredBackBufferWidth = 1024;
		_graphics.PreferredBackBufferHeight = 768;

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

	protected override void Draw(GameTime gameTime)
	{
		GraphicsDevice.Clear(Color.CornflowerBlue);
		
		_imRenderer.BeforeLayout(gameTime);
		
		ImGui.ShowDemoWindow();
		
		_imRenderer.AfterLayout();

		base.Draw(gameTime);
	}
}
