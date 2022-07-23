using System.Numerics;

namespace LibLunacy
{
	public class Gameplay
	{
		IGFile file;
		public Region[] regions;

		public Gameplay(AssetLoader al)
		{
			file = al.fm.igfiles["gameplay.dat"];

			if(al.fm.isOld)
			{
				regions = new Region[1];
				regions[0] = new Region(file, al);
			}
			else
			{
				IGFile.SectionHeader stringTableSection = file.QuerySection(0x25000);

				//gameplay.dat is a weird file in this version of the engine, the count field of section headers is the length and length field of section headers is 0

				file.sh.Seek(stringTableSection.offset + stringTableSection.count - 0x10);
				regions = new Region[file.sh.ReadUInt32()];

				uint regionTableOffset = file.sh.ReadUInt32();

				for(int i = 0; i < regions.Length; i++)
				{
					file.sh.Seek(regionTableOffset + 0x04 * i);
					
					string regionName = file.sh.ReadString(file.sh.ReadUInt32());
					Console.WriteLine($"Region {i}: {regionName}");
					regions[0] = new Region(al, regionName);
				}
			}
		}
	}

	public class Region
	{
		string name = "default";

		public Dictionary<ulong, CMobyInstance> mobyInstances = new Dictionary<ulong, CMobyInstance>();

		public class CMobyInstance
		{
			public Vector3 position;
			public Vector3 rotation;
			public float scale;
			public CMoby moby;

			public CMobyInstance(OldMobyInstance omi, AssetLoader al)
			{
				position = omi.position;
				rotation = omi.rotation;
				scale = omi.scale;
				moby = al.mobys[omi.mobyIndex];
			}
			public CMobyInstance(NewMobyInstance nmi, AssetLoader al, IGFile region)
			{
				position = nmi.position;
				rotation = nmi.rotation;
				scale = nmi.scale;
				
				region.sh.Seek(region.QuerySection(0x1C600).offset + 0x08 * nmi.mobyIndex);

				moby = al.mobys[region.sh.ReadUInt64()];
			}

		}


		[FileStructure(0x48)]
		public struct OldMobyInstance
		{
			[FileOffset(0x18)] public Vector3 position;
			[FileOffset(0x24)] public Vector3 rotation;		//ZYX euler in radians
			[FileOffset(0x30)] public float scale;
			[FileOffset(0x3C)] public ushort mobyIndex;
		}

		[FileStructure(0x50)]
		public struct NewMobyInstance
		{
			[FileOffset(0x00)] public ushort mobyIndex;
			[FileOffset(0x02)] public ushort groupIndex;
			[FileOffset(0x14)] public Vector3 position;
			[FileOffset(0x20)] public Vector3 rotation;		//ZYX euler in radians
			[FileOffset(0x2C)] public float scale;
		}
		[FileStructure(0x10)]
		public struct NewInstance
		{
			[FileOffset(0x00)] public ulong tuid;
			[FileOffset(0x08), Reference] public string name;
			[FileOffset(0x0C)] public ushort group;
		}

		public Region(IGFile file, AssetLoader al)
		{
			IGFile.SectionHeader mobyInstSections = file.QuerySection(0x7340);
			file.sh.Seek(mobyInstSections.offset);
			OldMobyInstance[] mobys = FileUtils.ReadStructureArray<OldMobyInstance>(file.sh, mobyInstSections.count);

			for(int i = 0; i < mobys.Length; i++)
			{
				mobyInstances.Add((ulong)i, new CMobyInstance(mobys[i], al));
			}
		}
		public Region(AssetLoader al, string regionName)
		{
			name = regionName;
			IGFile prius = (IGFile)al.fm.LoadFile($"{name}/gp_prius.dat", false);
			IGFile region = (IGFile)al.fm.LoadFile($"{name}/region.dat", false);

			IGFile.SectionHeader mobyInstSection = prius.QuerySection(0x25048);
			prius.sh.Seek(mobyInstSection.offset);
			NewMobyInstance[] mobys = FileUtils.ReadStructureArray<NewMobyInstance>(prius.sh, mobyInstSection.count);

			IGFile.SectionHeader mobyNamesSection = prius.QuerySection(0x2504C);
			prius.sh.Seek(mobyNamesSection.offset);
			NewInstance[] names = FileUtils.ReadStructureArray<NewInstance>(prius.sh, mobyInstSection.count);
			
			for(int i = 0; i < mobys.Length; i++)
			{
				mobyInstances.Add(names[i].tuid, new CMobyInstance(mobys[i], al, region));
			}
		}
	}
}