namespace LibLunacy
{
	public class AssetLoader
	{
		public FileManager fm;
		
		public Dictionary<ulong, CMoby> mobys = new Dictionary<ulong, CMoby>();
		public Dictionary<ulong, CShader> shaders = new Dictionary<ulong, CShader>();
		public Dictionary<uint, CTexture> textures = new Dictionary<uint, CTexture>();

		public AssetLoader(FileManager fileManager)
		{
			fm = fileManager;
		}

		public void LoadMobys()
		{
			if(fm.isOld) LoadMobysOld();
			else         LoadMobysNew();
		}
		private void LoadMobysOld()
		{
			IGFile main = fm.igfiles["main.dat"];
			IGFile.SectionHeader mobySection = main.QuerySection(0xD100);
			for(int i = 0; i < mobySection.count; i++)
			{
				mobys.Add((ulong)i, new CMoby(main, this, (uint)i));
			}
		}
		private void LoadMobysNew()
		{
			IGFile assetlookup = fm.igfiles["assetlookup.dat"];
			IGFile.SectionHeader mobySection = assetlookup.QuerySection(0x1D600);
			assetlookup.sh.Seek(mobySection.offset);
			AssetPointer[] mobyPtrs = FileUtils.ReadStructureArray<AssetPointer>(assetlookup.sh, mobySection.length / 0x10);
			Stream mobyStream = fm.rawfiles["mobys.dat"];
			for(int i = 0; i < mobyPtrs.Length; i++)
			{
				byte[] mobydat = new byte[mobyPtrs[i].length];
				mobyStream.Seek(mobyPtrs[i].offset, SeekOrigin.Begin);
				mobyStream.Read(mobydat, 0x00, (int)mobyPtrs[i].length);
				MemoryStream mobyms = new MemoryStream(mobydat);

				IGFile igmoby = new IGFile(mobyms);
				CMoby moby = new CMoby(igmoby, this);
				Console.WriteLine($"Moby {i.ToString("X04")} is {moby.name}");
				mobys.Add(mobyPtrs[i].tuid, moby);
			}
		}

		public void LoadShaders()
		{
			if(fm.isOld) LoadShadersOld();
			else         LoadShadersNew();
		}

		private void LoadShadersOld()
		{
			IGFile main = fm.igfiles["main.dat"];
			IGFile.SectionHeader shaderSection = main.QuerySection(0x5000);
			for(int i = 0; i < shaderSection.count; i++)
			{
				shaders.Add((ulong)i, new CShader(main, this, (uint)i));
			}
		}
		private void LoadShadersNew()
		{
			IGFile assetlookup = fm.igfiles["assetlookup.dat"];
			IGFile.SectionHeader shaderSection = assetlookup.QuerySection(0x1D100);
			assetlookup.sh.Seek(shaderSection.offset);
			AssetPointer[] shaderPtrs = FileUtils.ReadStructureArray<AssetPointer>(assetlookup.sh, shaderSection.length / 0x10);
			Stream shaderStream = fm.rawfiles["shaders.dat"];
			for(int i = 0; i < shaderPtrs.Length; i++)
			{
				byte[] shaderdat = new byte[shaderPtrs[i].length];
				shaderStream.Seek(shaderPtrs[i].offset, SeekOrigin.Begin);
				shaderStream.Read(shaderdat, 0x00, (int)shaderPtrs[i].length);
				MemoryStream shaderms = new MemoryStream(shaderdat);
				IGFile igshader = new IGFile(shaderms);
				CShader shader = new CShader(igshader, this);
				//Console.WriteLine($"shader {i.ToString("X04")} albedo offset is {shader.name}");
				shaders.Add(shaderPtrs[i].tuid, shader);
			}
		}

		public void LoadTextures()
		{
			if(fm.isOld) LoadTexturesOld();
			else         LoadTexturesNew();
		}

		private void LoadTexturesOld()
		{
			IGFile main = fm.igfiles["main.dat"];
			IGFile.SectionHeader textureSection = main.QuerySection(0x5200);
			for(int i = 0; i < textureSection.count; i++)
			{
				Console.WriteLine($"texture {i.ToString("X08")}");
				textures.Add((uint)(textureSection.offset + i * 0x20), new CTexture(fm, i));
			}
		}
		private void LoadTexturesNew()
		{
			IGFile assetlookup = fm.igfiles["assetlookup.dat"];
			IGFile.SectionHeader highmipSection = assetlookup.QuerySection(0x1D1C0);
			assetlookup.sh.Seek(highmipSection.offset);
			AssetPointer[] highmips = FileUtils.ReadStructureArray<AssetPointer>(assetlookup.sh, highmipSection.length / 0x10);

			for(int i = 0; i < highmips.Length; i++)
			{
				textures.Add((uint)highmips[i].tuid, new CTexture(fm, i));
			}
		}

		public void Dispose()
		{
			if(!fm.isOld)
			{
				foreach(KeyValuePair<ulong, CMoby> moby in mobys)
				{
					moby.Value.Dispose();
				}
			}
		}

		[FileStructure(0x10)]
		public struct AssetPointer
		{
			[FileOffset(0x00)] public ulong tuid;
			[FileOffset(0x08)] public uint offset;
			[FileOffset(0x0C)] public uint length;
		}
	}
}
