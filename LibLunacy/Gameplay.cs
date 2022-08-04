using System.Numerics;

namespace LibLunacy
{
	public class Gameplay
	{
		IGFile file;
		public Region[] regions;
		public Zone[] zones;

		public Gameplay(AssetLoader al)
		{
			file = al.fm.igfiles["gameplay.dat"];

			if(al.fm.isOld)
			{
				regions = new Region[1];
				regions[0] = new Region(file, al);

				zones = new Zone[1];
				zones[0] = new Zone(al.fm.igfiles["main.dat"], al);
			}
			else
			{
				//Loading regions

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
					regions[i] = new Region(al, regionName);
				}

				//Loading zones

				IGFile assetlookup = al.fm.igfiles["assetlookup.dat"];
				IGFile.SectionHeader zoneSection = assetlookup.QuerySection(0x1DA00);
				assetlookup.sh.Seek(zoneSection.offset);
				AssetLoader.AssetPointer[] zonePtrs = FileUtils.ReadStructureArray<AssetLoader.AssetPointer>(assetlookup.sh, zoneSection.length / 0x10);
				zones = new Zone[zonePtrs.Length];
				Stream zoneStream = al.fm.rawfiles["zones.dat"];
				for(int i = 0; i < zonePtrs.Length; i++)
				{
					byte[] zonedat = new byte[zonePtrs[i].length];
					zoneStream.Seek(zonePtrs[i].offset, SeekOrigin.Begin);
					zoneStream.Read(zonedat, 0x00, (int)zonePtrs[i].length);
					MemoryStream zonems = new MemoryStream(zonedat);
					IGFile igzone = new IGFile(zonems);
					Console.WriteLine($"zone {i}");
					zones[i] = new Zone(igzone, al);
				}
			}
		}
	}

	public class Zone
	{
		[FileStructure(0x80)]
		public struct TieInstance
		{
			[FileOffset(0x00)] public Matrix4x4 transformation;
			[FileOffset(0x40)] public Vector3 boundingPosition;
			[FileOffset(0x4C)] public float boundingRadius;
			[FileOffset(0x50)] public uint tie;					//Offset but used as a key into the assetloader ties dictionary on old engine, otherwise index into tuid array
		}

		[FileStructure(0x80)]
		public struct NewTFrag
		{
			[FileOffset(0x00)] public Matrix4x4 transformation;
			[FileOffset(0x40)] public uint indexOffset;
			[FileOffset(0x44)] public uint vertexOffset;
			[FileOffset(0x48)] public ushort indexCount;
			[FileOffset(0x4A)] public ushort vertexCount;

			public float[] vPositions;
			public uint[] indices;
		}


		public int index = 0;
		public Dictionary<ulong, CTieInstance> tieInstances = new Dictionary<ulong, CTieInstance>();
		public NewTFrag[] tfrags;


		public class CTieInstance
		{
			public string name = string.Empty;
			public Matrix4x4 transformation;
			public CTie tie;
			public Vector3 boundingPosition;
			public float boundingRadius;
			public CTieInstance(TieInstance instance, AssetLoader al, IGFile file)
			{
				transformation = instance.transformation;
				if(al.fm.isOld)
				{
					tie = al.ties[instance.tie];
				}
				else
				{
					file.sh.Seek(file.QuerySection(0x7200).offset + 0x08 * instance.tie);

					tie = al.ties[file.sh.ReadUInt64()];
				}
				boundingPosition = instance.boundingPosition;
				boundingRadius = instance.boundingRadius;
			}
		}

		public Zone(IGFile file, AssetLoader al)
		{
			IGFile.SectionHeader tieInstSection;
			AssetLoader.AssetPointer[] newnames = null;
			DebugFile.DebugInstanceName[] oldnames = null;

			if(al.fm.isOld)
			{
				tieInstSection = file.QuerySection(0x9240);
				if(al.fm.debug != null)
				{
					oldnames = al.fm.debug.GetTieInstanceNames();
				}
			}
			else
			{
				IGFile.SectionHeader tieNameSection = file.QuerySection(0x72C0);
				file.sh.Seek(tieNameSection.offset);
				Console.WriteLine($"names @ {tieNameSection.offset}");
				newnames = FileUtils.ReadStructureArray<AssetLoader.AssetPointer>(file.sh, tieNameSection.count);
				
				tieInstSection = file.QuerySection(0x7240);
			}

			file.sh.Seek(tieInstSection.offset);
			TieInstance[] ties = FileUtils.ReadStructureArray<TieInstance>(file.sh, tieInstSection.count);

			for(int i = 0; i < ties.Length; i++)
			{
				tieInstances.Add((ulong)i, new CTieInstance(ties[i], al, file));
				if(al.fm.isOld)
				{
					if(al.fm.debug != null) tieInstances.Last().Value.name = oldnames[i].name;
					else                    tieInstances.Last().Value.name = $"Tie_{i}";
				}
				else
				{
					tieInstances.Last().Value.name = file.sh.ReadString(newnames[i].offset);
				}
			}

			//tfrags = new NewTFrag[0];
			LoadTFrags(file, al);
		}

		private void LoadTFrags(IGFile file, AssetLoader al)
		{
			IGFile.SectionHeader tfragSection = file.QuerySection(0x6200);

			IGFile geometryFile;

			IGFile.SectionHeader vertexSection;
			IGFile.SectionHeader indexSection;

			if(al.fm.isOld)
			{
				geometryFile = al.fm.igfiles["vertices.dat"];
				vertexSection = file.QuerySection(0x9000);
				indexSection = file.QuerySection(0x9100);
			}
			else
			{
				geometryFile = file;
				vertexSection = file.QuerySection(0x6000);
				indexSection = file.QuerySection(0x6100);
			}

			file.sh.Seek(tfragSection.offset);
			tfrags = FileUtils.ReadStructureArray<NewTFrag>(file.sh, tfragSection.count);
			for(int i = 0; i < tfrags.Length; i++)
			{
				tfrags[i].vPositions = new float[tfrags[i].vertexCount * 3];
				tfrags[i].indices = new uint[tfrags[i].indexCount];

				for(int j = 0; j < tfrags[i].vertexCount; j++)
				{
					geometryFile.sh.Seek(vertexSection.offset + tfrags[i].vertexOffset + 0x18 * j);
					float x = file.sh.ReadInt16() / (float)0x7FFF;
					float y = file.sh.ReadInt16() / (float)0x7FFF;
					float z = file.sh.ReadInt16() / (float)0x7FFF;
					tfrags[i].vPositions[j * 3 + 0] = x;
					tfrags[i].vPositions[j * 3 + 1] = y;
					tfrags[i].vPositions[j * 3 + 2] = z;
				}

				if(al.fm.isOld) geometryFile.sh.Seek(indexSection.offset + tfrags[i].indexOffset);
				else			geometryFile.sh.Seek(indexSection.offset + tfrags[i].indexOffset);

				for(int j = 0; j < tfrags[i].indexCount; j++)
				{
					tfrags[i].indices[j] = file.sh.ReadUInt16();
				}
			}
		}
	}

	public class Region
	{
		public string name = "default";

		public Dictionary<ulong, CMobyInstance> mobyInstances = new Dictionary<ulong, CMobyInstance>();

		public class CMobyInstance
		{
			public Vector3 position;
			public Vector3 rotation;
			public float scale;
			public CMoby moby;
			public string name;

			public CMobyInstance(OldMobyInstance omi, AssetLoader al)
			{
				position = omi.position;
				rotation = omi.rotation;
				scale = omi.scale;
				moby = al.mobys[omi.mobyIndex];
			}
			public CMobyInstance(NewMobyInstance nmi, NewInstance ni, AssetLoader al, IGFile region)
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

			DebugFile.DebugInstanceName[] names = null;
			if(al.fm.debug != null)
			{
				names = al.fm.debug.GetMobyInstanceNames();
			}

			for(int i = 0; i < mobys.Length; i++)
			{
				mobyInstances.Add((ulong)i, new CMobyInstance(mobys[i], al));
				if(names != null)
				{
					mobyInstances.Last().Value.name = names[i].name;
				}
				else
				{
					mobyInstances.Last().Value.name = $"Moby_{mobys[i].mobyIndex.ToString("X04")}_Instance_{i}";
				}
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
				mobyInstances.Add(names[i].tuid, new CMobyInstance(mobys[i], names[i], al, region));
				mobyInstances.Last().Value.name = names[i].name;
			}
		}
	}
}