using System.Numerics;
using System.Runtime.CompilerServices;

namespace LibLunacy
{
	public class CZone
	{
		[FileStructure(0x80)]
		public struct TieInstance
		{
			[FileOffset(0x00)] public Matrix4x4 transformation;
			[FileOffset(0x40)] public Vector3 boundingPosition;
			[FileOffset(0x4C)] public float boundingRadius;
			[FileOffset(0x50)] public uint tie;					//Offset but used as k key into the assetloader ties dictionary on old engine, otherwise index into tuid array
		}

		[FileStructure(0x80)]
		public struct NewTFrag
		{
			[FileOffset(0x00)] public Matrix4x4 transformation;
			[FileOffset(0x40)] public uint indexOffset;
			[FileOffset(0x44)] public uint vertexOffset;
			[FileOffset(0x48)] public ushort indexCount;
			[FileOffset(0x4A)] public ushort vertexCount;
			[FileOffset(0x4C)] public float unk1;
			[FileOffset(0x54)] public Vector3 unk2;
			[FileOffset(0x5C)] public float unk3;
			[FileOffset(0x68)] public Vector3 unk4;
			[FileOffset(0x6C)] public float unk5; // Always 1 or 0.2
			[FileOffset(0x70)] public Vector3 scale;
			[FileOffset(0x7C)] public float unk6;  // Always 0
			
			// The legend tells that the shader index of the UFrag is hidden somewhere in this mess.

			public float[] vPositions;
			public uint[] indices;
		}

        [FileStructure(0x18)]
        public struct UFragVertex
        {
            [FileOffset(0x00)] public short x;
            [FileOffset(0x02)] public short y;
            [FileOffset(0x04)] public short z;
            [FileOffset(0x06)] public ushort unkConst;  // Not an interesting thing afaik
            [FileOffset(0x08)] public Half UVx;
            [FileOffset(0x0A)] public Half UVy;
            [FileOffset(0x0C)] public float unk1;
            [FileOffset(0x10)] public Half unk2;
			[FileOffset(0x12)] public Half unk3;
			[FileOffset(0x14)] public short unkShort;
			[FileOffset(0x16)] public ushort unk4;
        }

		[FileStructure(0x10)]
		public struct UFragScalar
		{
			[FileOffset(0x00)] public uint index;
			[FileOffset(0x04)] public float x;
			[FileOffset(0x08)] public float y;
			[FileOffset(0x0C)] public uint padder;
		}

		/// <summary>
		/// For debug purposes.
		/// </summary>
		/// <param name="obj">Any object</param>
		/// <returns>An accurate string representation of the object</returns>
        public static string ToString(object obj)
        {
            var fields = obj.GetType().GetFields();
            var sb = new StringBuilder();
            sb.AppendLine($"{obj.GetType().Name} {{");
            foreach (var field in fields)
            {
                var val = field.GetValue(obj);
                sb.AppendLine($"\t{field.FieldType.Name} {field.Name}: {val};");
            }
            sb.AppendLine("}");
            return sb.ToString();
        }


        public int index = 0;
		public Dictionary<ulong, CTieInstance> tieInstances = new Dictionary<ulong, CTieInstance>();
		public NewTFrag[] tfrags;
		public UFragScalar[] ufragscales;
		public string name;

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

		public CZone(IGFile file, AssetLoader al)
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
			IGFile.SectionHeader scalarSection;

			if(al.fm.isOld)
			{
				geometryFile = al.fm.igfiles["vertices.dat"];
				vertexSection = file.QuerySection(0x9000);
				indexSection = file.QuerySection(0x9100);
				scalarSection = file.QuerySection(0x9400);
			}
			else
			{
				geometryFile = file;
				vertexSection = file.QuerySection(0x6000);
				indexSection = file.QuerySection(0x6100);
				scalarSection = file.QuerySection(0x6400);
			}

            geometryFile.sh.Seek(scalarSection.offset);
            ufragscales = geometryFile.sh.ReadStructArray<UFragScalar>(scalarSection.count);

            geometryFile.sh.Seek(tfragSection.offset);
			tfrags = FileUtils.ReadStructureArray<NewTFrag>(file.sh, tfragSection.count);
            for (int i = 0; i < tfrags.Length; i++)
			{
				tfrags[i].vPositions = new float[tfrags[i].vertexCount * 3];
				tfrags[i].indices = new uint[tfrags[i].indexCount];

				geometryFile.sh.Seek(vertexSection.offset + tfrags[i].vertexOffset);
				var uFragVertices = geometryFile.sh.ReadStructArray<UFragVertex>(tfrags[i].vertexCount);

				//Console.WriteLine(ToString(uFragVertices[1]));

				for(int j = 0; j < tfrags[i].vertexCount; j++)
				{
					var uFragVertex = uFragVertices[j];
					tfrags[i].vPositions[j * 3 + 1] = uFragVertex.y;
					tfrags[i].vPositions[j * 3 + 2] = uFragVertex.z;
					tfrags[i].vPositions[j * 3 + 0] = uFragVertex.x;
				}

                geometryFile.sh.Seek(indexSection.offset + tfrags[i].indexOffset);

                for(int j = 0; j < tfrags[i].indexCount; j++)
				{
					tfrags[i].indices[j] = file.sh.ReadUInt16();
				}

				//Console.WriteLine(ToString(ufragscales[i]));
			}
		}
	}
}
