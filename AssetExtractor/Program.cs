using LibLunacy;

namespace AssetExtractor
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			FileManager fm = new FileManager();
			fm.LoadFolder(args[0]);
			AssetLoader al = new AssetLoader(fm);
			al.LoadTextures();
			al.LoadShaders();
			al.LoadMobys();
			al.LoadTies();
			foreach(KeyValuePair<ulong, CMoby> mobys in al.mobys)
			{
				string exportFilePath = $"mobys/{Path.ChangeExtension(mobys.Value.name, "obj")}";
				Directory.CreateDirectory(Path.GetDirectoryName(exportFilePath));
				mobys.Value.ExportToObj(exportFilePath);
			}
			foreach(KeyValuePair<ulong, CTie> tie in al.ties)
			{
				string exportFilePath = $"ties/{Path.ChangeExtension(tie.Value.name, "obj")}";
				Directory.CreateDirectory(Path.GetDirectoryName(exportFilePath));
				tie.Value.ExportToObj(exportFilePath);
			}
		}
	}
}