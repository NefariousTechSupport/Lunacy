namespace LibLunacy
{
	public class IGFile
	{
		public uint sectionCount;
		public uint headerLength;
		public uint fileLength;
		public uint unknown;
		public SectionHeader[] sections;

		public StreamHelper sh;


		[FileStructure(0x10)]
		public struct SectionHeader
		{
			[FileOffset(0x00)] public uint id;
			[FileOffset(0x04)] public uint offset;
			[FileOffset(0x08)] public uint count;
			[FileOffset(0x0C)] public uint length;
		}

		public IGFile(Stream data)
		{
			sh = new StreamHelper(data, StreamHelper.Endianness.Big);

			sh.Seek(0x00);

			uint magic1 = sh.ReadUInt32();
			uint type = sh.ReadUInt32();

			if(magic1 == 0x57484749) sh._endianness = StreamHelper.Endianness.Little;
			else if(magic1 != 0x49474857) throw new System.Exception("Invalid IGHW file");

			if(type == 0x00010001)
			{
				sectionCount = sh.ReadUInt32();
				headerLength = sh.ReadUInt32();
				fileLength = sh.ReadUInt32();
				unknown = sh.ReadUInt32();

				sh.Seek(0x20);
			}
			else if(type == 0x02)
			{
				sectionCount = sh.ReadUInt32();

				sh.Seek(0x10);
			}

			sections = FileUtils.ReadStructureArray<SectionHeader>(sh, sectionCount);

			for(int i = 0; i < sectionCount; i++)
			{
				sections[i].count &= ~0x10000000u;
			}
		}

		public SectionHeader QuerySection(uint id)
		{
			if(sections.Any(x => x.id == id))
			{
				return sections.First(x => x.id == id);
			}
			else return new SectionHeader();
		}
		public void Dispose()
		{
			sh.Close();
			sh.Dispose();
		}
	}
}
