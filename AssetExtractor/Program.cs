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
				string exportFilePath = $"{args[0]}/assets/mobys/{Path.ChangeExtension(mobys.Value.name, "obj")}";
				Directory.CreateDirectory(Path.GetDirectoryName(exportFilePath));
				Console.WriteLine(exportFilePath);
				mobys.Value.ExportToObj(exportFilePath);
			}
			foreach(KeyValuePair<ulong, CTie> tie in al.ties)
			{
				string exportFilePath = $"{args[0]}/assets/ties/{Path.ChangeExtension(tie.Value.name, "obj")}";
				Directory.CreateDirectory(Path.GetDirectoryName(exportFilePath));
				Console.WriteLine(exportFilePath);
				tie.Value.ExportToObj(exportFilePath);
			}
			foreach(KeyValuePair<uint, CTexture> texture in al.textures)
			{
				string textureName = texture.Value.name;
				if(textureName == string.Empty)
				{
					textureName = $"Texture_{texture.Value.id}";
				}
				Console.WriteLine(textureName);
				if(textureName[1] == ':')
				{
					textureName = textureName.Substring(3);
				}
				string exportFilePath = $"{args[0]}/assets/textures/{textureName}.dds";
				Directory.CreateDirectory(Path.GetDirectoryName(exportFilePath));
				texture.Value.ExportToDDS(File.Open(exportFilePath, FileMode.Create), false);
			}
		}
	}
}