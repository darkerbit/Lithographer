namespace Lithographer;

public static class FfmpegRunner
{
	public static bool Running { get; private set; }
	
	public static void Run(string _image, string _music, string _out)
	{
		Running = true;
	}
}
