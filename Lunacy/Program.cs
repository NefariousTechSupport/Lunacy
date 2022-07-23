using LibLunacy;

using OpenTK;

namespace Lunacy
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			Window wnd = new Window(
				new GameWindowSettings()
				{
					IsMultiThreaded = false
				},
				new NativeWindowSettings()
				{
					Size = new Vector2i(1280, 720),
					Title = "Lunacy Level Editor",
					Flags = ContextFlags.ForwardCompatible
				},
				args[0]
			);

			wnd.Run();
		}
	}
}