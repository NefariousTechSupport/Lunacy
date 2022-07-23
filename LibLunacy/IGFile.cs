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
			uint magic2 = sh.ReadUInt32();

			if(magic1 == 0x57484749) sh._endianness = StreamHelper.Endianness.Little;
			else if(magic1 != 0x49474857) throw new System.Exception("Invalid IGHW file");

			sectionCount = sh.ReadUInt32();
			headerLength = sh.ReadUInt32();
			fileLength = sh.ReadUInt32();
			unknown = sh.ReadUInt32();

			/*if(data is SubStream)
			{
				Console.WriteLine($"{sectionCount.ToString("X08")} @ {(data as SubStream)._base.Position.ToString("X08")}");
			}*/

			sh.Seek(0x20);

			sections = FileUtils.ReadStructureArray<SectionHeader>(sh, sectionCount);

			for(int i = 0; i < sectionCount; i++)
			{
				sections[i].count &= ~0x10000000u;

				//Debug.Log($"Sections {i.ToString("X02")}: {sections[i].id.ToString("X08")}, {sections[i].offset.ToString("X08")}, {sections[i].length.ToString("X08")}, {sections[i].count.ToString("X08")}");

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
