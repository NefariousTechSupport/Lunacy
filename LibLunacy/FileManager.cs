namespace LibLunacy
{
	public class FileManager
	{
		public Dictionary<string, IGFile> igfiles = new Dictionary<string, IGFile>();
		public Dictionary<string, Stream> rawfiles = new Dictionary<string, Stream>();

		public bool isOld { get; private set; }

		public void LoadFolder(string folderPath)
		{
			DirectoryInfo di = new DirectoryInfo(folderPath);
			FileInfo[] files = di.GetFiles();
			isOld = files.Any(x => x.Name == "main.dat");
			if(isOld)
			{
				//Load IGFiles
				LoadFile("main.dat", folderPath, false);
				LoadFile("vertices.dat", folderPath, false);

				//Load raw files
				LoadFile("textures.dat", folderPath, true);
			}
			else
			{
				//Load IGFiles
				LoadFile("assetlookup.dat", folderPath, false);
				//LoadFile("gameplay.dat", folderPath, false);

				//Load raw files, a lot of these are actually a bunch of IGFiles, however the files themselves are not IGFiles
				LoadFile("mobys.dat", folderPath, true);
				//LoadFile("ties.dat", folderPath, true);
				LoadFile("textures.dat", folderPath, true);
				LoadFile("highmips.dat", folderPath, true);
				LoadFile("shaders.dat", folderPath, true);
				//LoadFile("zones.dat", folderPath, true);
			}
		}

		private void LoadFile(string name, string folderPath, bool isRaw)
		{
			FileStream fs = File.Open($"{folderPath}/{name}", FileMode.Open, FileAccess.Read);
			MemoryStream ms = new MemoryStream((int)fs.Length);
			fs.CopyTo(ms);
			fs.Close();
			if(isRaw)
			{
				rawfiles.Add(name, ms);
			}
			else
			{
				igfiles.Add(name, new IGFile(ms));
			}
		}
	}
}