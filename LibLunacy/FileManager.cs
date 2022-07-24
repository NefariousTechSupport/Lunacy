namespace LibLunacy
{
	public class FileManager
	{
		public string folderPath = string.Empty;

		public Dictionary<string, IGFile> igfiles = new Dictionary<string, IGFile>();
		public Dictionary<string, Stream> rawfiles = new Dictionary<string, Stream>();

		public bool isOld { get; private set; }

		public void LoadFolder(string folderPath)
		{
			this.folderPath = folderPath;
			DirectoryInfo di = new DirectoryInfo(folderPath);
			FileInfo[] files = di.GetFiles();
			isOld = files.Any(x => x.Name == "main.dat");
			if(isOld)
			{
				//Load IGFiles
				LoadFile("main.dat", false);
				LoadFile("vertices.dat", false);
				LoadFile("gameplay.dat", false);

				//Load raw files
				LoadFile("textures.dat", true);
			}
			else
			{
				//Load IGFiles
				LoadFile("gameplay.dat", false);
				LoadFile("assetlookup.dat", false);

				//Load raw files, a lot of these are actually a bunch of IGFiles, however the files themselves are not IGFiles
				LoadFile("mobys.dat", true);
				//LoadFile("ties.dat", true);
				LoadFile("textures.dat", true);
				LoadFile("highmips.dat", true);
				LoadFile("shaders.dat", true);
				//LoadFile("zones.dat", true);
			}
		}

		public object LoadFile(string name, bool isRaw)
		{
			//If anyone's wondering, the following basically doubles ram usage but doesn't latch onto files, useful for debugging
			/*FileStream fs = File.Open($"{folderPath}/{name}", FileMode.Open, FileAccess.Read);
			MemoryStream ms = new MemoryStream((int)fs.Length);
			fs.CopyTo(ms);
			fs.Close();*/

			//The following doesn't use as much ram but holds onto files
			FileStream ms = File.Open($"{folderPath}/{name}", FileMode.Open, FileAccess.Read);

			if(isRaw)
			{
				rawfiles.Add(name, ms);
				return ms;
			}
			else
			{
				IGFile file = new IGFile(ms);
				igfiles.Add(name, file);
				return file;
			}
		}
	}
}