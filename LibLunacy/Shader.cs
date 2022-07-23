namespace LibLunacy
{
	public class CShader
	{
		[FileStructure(0x80)]
		public struct OldShader
		{
			//These could be OldTextureReference references but i've instead made them just uints so that it can work better with the CTexture class
			[FileOffset(0x00)] public uint albedoOffset;
			[FileOffset(0x04)] public uint normalOffset;
			[FileOffset(0x08)] public uint expensiveOffset;
		}
		[FileStructure(0x80)]
		public struct NewShader
		{
			[FileOffset(0x00)] public uint albedoIndex;
			[FileOffset(0x04)] public uint normalIndex;
			[FileOffset(0x08)] public uint expensiveIndex;
		}
		[FileStructure(0x40)]
		public struct NewReferences
		{
			[FileOffset(0x00)] public ulong thisTuid;
			[FileOffset(0x10)] public uint albedoTuid;
			[FileOffset(0x14)] public uint normalTuid;
			[FileOffset(0x18)] public uint expensiveTuid;

			public uint TextureCount => 3;
		}

		IGFile file;

		public CTexture? albedo = null;
		public CTexture? normal = null;
		public CTexture? expensive = null;

		public CShader(IGFile file, AssetLoader al, uint index = 0)
		{
			this.file = file;

			IGFile.SectionHeader section = file.QuerySection(0x5000);

			file.sh.Seek(section.offset + 0x80 * index);

			if(al.fm.isOld)
			{
				OldShader oshader = FileUtils.ReadStructure<OldShader>(file.sh);

				Console.WriteLine($"tex offset for index {index.ToString("X08")}: {oshader.albedoOffset.ToString("X08")}");

				if(oshader.albedoOffset != 0) albedo = al.textures[oshader.albedoOffset];
				if(oshader.normalOffset != 0) normal = al.textures[oshader.normalOffset];
				if(oshader.expensiveOffset != 0) expensive = al.textures[oshader.expensiveOffset];
			}
			else
			{
				file.sh.Seek(section.offset, SeekOrigin.Begin);
				NewShader nshader = FileUtils.ReadStructure<NewShader>(file.sh);
				IGFile.SectionHeader refSection = file.QuerySection(0x5D00);
				file.sh.Seek(refSection.offset, SeekOrigin.Begin);
				NewReferences refs = FileUtils.ReadStructure<NewReferences>(file.sh);

				Console.WriteLine($"Handling shader {refs.thisTuid.ToString("X016")}'s dependancies");

				if(refs.albedoTuid != 0 && al.textures.ContainsKey(refs.albedoTuid))       albedo    = al.textures[refs.albedoTuid];
				if(refs.normalTuid != 0 && al.textures.ContainsKey(refs.normalTuid))       normal    = al.textures[refs.normalTuid];
				if(refs.expensiveTuid != 0 && al.textures.ContainsKey(refs.expensiveTuid)) expensive = al.textures[refs.expensiveTuid];
			}
		}
	}
}