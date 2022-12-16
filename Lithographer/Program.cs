namespace Lithographer
{
	internal static class Program
	{
		internal static void Main()
		{
			using (LithographerGame game = new LithographerGame())
			{
				game.Run();
			}
		}
	}
}
