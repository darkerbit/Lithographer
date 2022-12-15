namespace Lithographer
{
	internal static class Program
	{
		internal static void Main()
		{
			using (var game = new LithographerGame())
			{
				game.Run();
			}
		}
	}
}
