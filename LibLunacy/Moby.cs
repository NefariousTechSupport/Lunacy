using System.Numerics;

namespace LibLunacy
{
	public class CMoby
	{
		[FileStructure(0x40)]
		public struct MobyMesh
		{
			[FileOffset(0x00)] public uint indexIndex;
			[FileOffset(0x04)] public uint vertexOffset;
			[FileOffset(0x08)] public ushort shaderIndex;
			[FileOffset(0x0A)] public ushort vertexCount;
			[FileOffset(0x0C)] public byte boneMapIndexCount;
			[FileOffset(0x0D)] public byte vertexType;
			[FileOffset(0x0E)] public byte boneMapIndex;
			[FileOffset(0x12)] public ushort indexCount;
			[FileOffset(0x20)] public uint boneMap;				//Should turn this into a reference

			//Geometry data included here for ease of access
			public float[] vPositions;
			public float[] vTexCoords;
			//public float[] vNormals;
			public uint[] indices;

			public CShader shader;
		}
		[FileStructure(0x08)]
		public struct Bangle
		{
			[FileOffset(0x00), Reference("MetadataCount")] public MobyMesh[] meshes;
			[FileOffset(0x04)] public uint count;

			public uint MetadataCount
			{
				get => count;
			}
		}

		[FileStructure(0xC0)]
		public struct OldMoby
		{
			[FileOffset(0x00)] public Vector3 boundingSpherePosition;
			[FileOffset(0x0C)] public float boundingSphereRotation;
			[FileOffset(0x18)] public ushort bangleCount1;
			[FileOffset(0x1A)] public ushort bangleCount2;
			[FileOffset(0x28), Reference("BangleCount")] public Bangle[] bangles;
			[FileOffset(0x34)] public uint indexOffset;
			[FileOffset(0x38)] public uint vertexOffset;
			[FileOffset(0x3C)] public float scale;

			public uint BangleCount
			{
				get => bangleCount1;//(uint)(bangleCount1 * (bangleCount2 + 1));
			}
		}

		[FileStructure(0x100)]
		public struct NewMoby
		{
			[FileOffset(0x00)] public Vector3 boundingSpherePosition;
			[FileOffset(0x0C)] public float boundingSphereRotation;
			[FileOffset(0x18)] public ushort bangleCount1;
			[FileOffset(0x1A)] public ushort bangleCount2;
			[FileOffset(0x24), Reference("BangleCount")] public Bangle[] bangles;
			[FileOffset(0x70)] public float scale;

			public uint BangleCount
			{
				get => bangleCount1;//(uint)(bangleCount1 * (bangleCount2 + 1));
			}
		}

		private enum VertexAttriubute
		{
			Positions,
			TexCoords,
		}

		public Bangle[] bangles;
		public string name;
		public float scale;
		public IGFile file;

		//public List<int> indices = new List<int>();
		//public List<float> vps = new List<float>();

		public uint MetadataCount
		{
			get
			{
				uint count = 0;
				for(int i = 0; i < bangles.Length; i++)
				{
					count += (uint)bangles[i].meshes.Length;
				}
				return count;
			}
		}

		public CMoby(IGFile file, AssetLoader al, uint index = 0)
		{
			this.file = file;
			IGFile.SectionHeader section = file.QuerySection(0xD100);
			StreamHelper vertexStream;
			StreamHelper indexStream;
			if(section.length == 0x100)
			{
				file.sh.Seek(section.offset);
				NewMoby nmoby = FileUtils.ReadStructure<NewMoby>(file.sh);

				Console.WriteLine($"nmoby.bangles.Length {nmoby.bangles.Length}");

				bangles = nmoby.bangles;
				IGFile.SectionHeader namesection = file.QuerySection(0xD200);
				name = file.sh.ReadString(namesection.offset);
				scale = nmoby.scale * 0x8000;

				IGFile.SectionHeader vertexsection = file.QuerySection(0xE200);
				//SubStream vertexms = new SubStream(file.sh.BaseStream, vertexsection.offset, vertexsection.length);
				file.sh.Seek(vertexsection.offset);
				MemoryStream vertexms = new MemoryStream(file.sh.ReadBytes(vertexsection.length));
				vertexStream = new StreamHelper(vertexms, file.sh._endianness);

				IGFile.SectionHeader indexsection = file.QuerySection(0xE100);
				//SubStream indexms = new SubStream(file.sh.BaseStream,indexsection.offset,indexsection.length);
				file.sh.Seek(indexsection.offset);
				MemoryStream indexms = new MemoryStream(file.sh.ReadBytes(indexsection.length));
				indexStream = new StreamHelper(indexms, file.sh._endianness);
			}
			else
			{
				file.sh.Seek(section.offset + 0xC0 * index);
				OldMoby omoby = FileUtils.ReadStructure<OldMoby>(file.sh);

				bangles = omoby.bangles;
				name = $"Moby_{index.ToString("X04")}";
				scale = omoby.scale * 0x8000;

				IGFile vertexFile = al.fm.igfiles["vertices.dat"];

				MobyMesh lastMesh = omoby.bangles.Last(x => x.count != 0).meshes.Last();
				Stream vertexFileToUse = null;
				Stream indexFileToUse = null;

				if((omoby.vertexOffset & 0x80000000) != 0)
				{
					vertexFileToUse = vertexFile.sh.BaseStream;
					vertexFileToUse.Seek(vertexFile.QuerySection(0x9000).offset, SeekOrigin.Begin);
				}
				else
				{
					vertexFileToUse = al.fm.rawfiles["textures.dat"];
					vertexFileToUse.Seek(0, SeekOrigin.Begin);
				}
				vertexFileToUse.Seek(omoby.vertexOffset & ~0x80000000, SeekOrigin.Current);
				uint vertexSize = lastMesh.vertexOffset + lastMesh.vertexCount * (lastMesh.vertexType == 1 ? 0x1Cu : 0x14u);
				byte[] vertexdata = new byte[vertexSize];
				vertexFileToUse.Read(vertexdata);
				MemoryStream vertexms = new MemoryStream(vertexdata);

				if((omoby.indexOffset & 0x80000000) != 0)
				{
					indexFileToUse = vertexFile.sh.BaseStream;
					indexFileToUse.Seek(vertexFile.QuerySection(0x9100).offset, SeekOrigin.Begin);
				}
				else
				{
					indexFileToUse = al.fm.rawfiles["textures.dat"];
					indexFileToUse.Seek(0, SeekOrigin.Begin);
				}
				indexFileToUse.Seek(omoby.indexOffset & ~0x80000000, SeekOrigin.Current);
				uint indexSize = (lastMesh.indexIndex + lastMesh.indexCount) * 2;
				byte[] indexdata = new byte[indexSize];
				indexFileToUse.Read(indexdata);
				MemoryStream indexms = new MemoryStream(indexdata);

				vertexStream = new StreamHelper(vertexms, file.sh._endianness);
				indexStream = new StreamHelper(indexms, file.sh._endianness);
			}

			InitializeBuffers(vertexStream, indexStream);
			vertexStream.Close();
			indexStream.Close();

			LoadDependancies(al);
		}
		private void InitializeBuffers(StreamHelper vertexStream, StreamHelper indexStream)
		{
			for(int i = 0; i < bangles.Length; i++)
			{
				for(int j = 0; j < bangles[i].count; j++)
				{
					bangles[i].meshes[j].indices = new uint[bangles[i].meshes[j].indexCount];
					indexStream.Seek(bangles[i].meshes[j].indexIndex * 2);
					for(int k = 0; k < bangles[i].meshes[j].indexCount; k++)
					{
						bangles[i].meshes[j].indices[k] = indexStream.ReadUInt16();
					}

					int stride = bangles[i].meshes[j].vertexType == 1 ? 0x1C : 0x14;
					bangles[i].meshes[j].vPositions = new float[bangles[i].meshes[j].vertexCount * 3];
					bangles[i].meshes[j].vTexCoords = new float[bangles[i].meshes[j].vertexCount * 2];
					//bangles[i].meshes[j].vNormals = new float[bangles[i].meshes[j].vertexCount * 3];
					for(int k = 0; k < bangles[i].meshes[j].vertexCount; k++)
					{
						vertexStream.Seek(bangles[i].meshes[j].vertexOffset + stride * k + 0x00);
						bangles[i].meshes[j].vPositions[k * 3 + 0] = (vertexStream.ReadInt16() / (float)0x7FFF) * scale;
						bangles[i].meshes[j].vPositions[k * 3 + 1] = (vertexStream.ReadInt16() / (float)0x7FFF) * scale;
						bangles[i].meshes[j].vPositions[k * 3 + 2] = (vertexStream.ReadInt16() / (float)0x7FFF) * scale;

						vertexStream.Seek(bangles[i].meshes[j].vertexOffset + stride * k + (bangles[i].meshes[j].vertexType == 1 ? 0x10 : 0x08));

						bangles[i].meshes[j].vTexCoords[k * 2 + 0] = (float)vertexStream.ReadHalf();
						bangles[i].meshes[j].vTexCoords[k * 2 + 1] = (float)vertexStream.ReadHalf();

						/*vertexStream.Seek(bangles[i].meshes[j].vertexOffset + stride * (k+1) - 0x04);
						vertexStream.bitPosition = 1;
						int vnx = (int)vertexStream.ReadIntN(11);
						int vny = (int)vertexStream.ReadIntN(10);
						int vnz = (int)vertexStream.ReadIntN(10);
						bangles[i].meshes[j].vNormals[k * 3 + 0] = (vnx / 511f) * 2 - 2;
						bangles[i].meshes[j].vNormals[k * 3 + 1] = (vny / 511f) * 2 - 2;
						bangles[i].meshes[j].vNormals[k * 3 + 2] = (vnz / 511f) * 2 - 2;*/
					}
				}
			}
		}

		public void LoadDependancies(AssetLoader al)
		{
			IGFile.SectionHeader shaderSection;
			if(al.fm.isOld)
			{
				shaderSection = al.fm.igfiles["main.dat"].QuerySection(0x5000);
			}
			else
			{
				shaderSection = file.QuerySection(0x5600);
			}
			for(int i = 0; i < bangles.Length; i++)
			{
				for(int j = 0; j < bangles[i].count; j++)
				{
					uint shaderIndex = bangles[i].meshes[j].shaderIndex;

					if(al.fm.isOld)
					{
						bangles[i].meshes[j].shader = al.shaders[shaderIndex];
					}
					else
					{
						file.sh.Seek(shaderSection.offset + shaderIndex * 8);
						bangles[i].meshes[j].shader = al.shaders[file.sh.ReadUInt64()];
					}
				}
			}
		}

		public void ExportToObj(string filePath)
		{
			uint maxIndex = 0;

			StringBuilder obj = new StringBuilder();

			obj.Append($"mtllib unused.mtl\n");
			for(int i = 0; i < bangles.Length; i++)
			{
				Console.WriteLine($"Bangle {i+1}/{bangles.Length}");
				obj.Append($"o Bangle_{i}\n");
				for(int j = 0; j < bangles[i].count; j++)
				{
					Console.WriteLine($"Submesh {j+1}/{bangles[i].count}");

					for(int k = 0; k < bangles[i].meshes[j].vertexCount; k++)
					{
						obj.Append($"v {bangles[i].meshes[j].vPositions[k * 3].ToString("F8")} {bangles[i].meshes[j].vPositions[k * 3 + 1].ToString("F8")} {bangles[i].meshes[j].vPositions[k * 3 + 2].ToString("F8")}\n");
						obj.Append($"vt {bangles[i].meshes[j].vTexCoords[k * 2].ToString("F8")} {bangles[i].meshes[j].vTexCoords[k * 2 + 1].ToString("F8")}\n");
						//obj.Append($"vn {bangles[i].meshes[j].vNormals[k * 3].ToString("F8")} {bangles[i].meshes[j].vNormals[k * 3 + 1].ToString("F8")} {bangles[i].meshes[j].vNormals[k * 3 + 2].ToString("F8")}\n");
					}

					obj.Append($"usemtl Shader_{bangles[i].meshes[j].shaderIndex}\n");

					for(int k = 0; k < bangles[i].meshes[j].indexCount; k += 3)
					{
						string i1 = (bangles[i].meshes[j].indices[k + 0] + maxIndex + 1).ToString();
						string i2 = (bangles[i].meshes[j].indices[k + 1] + maxIndex + 1).ToString();
						string i3 = (bangles[i].meshes[j].indices[k + 2] + maxIndex + 1).ToString();
						//obj.Append($"f {i1}/{i1}/{i1} {i2}/{i2}/{i2} {i3}/{i3}/{i3}\n");
						obj.Append($"f {i1}/{i1} {i2}/{i2} {i3}/{i3}\n");
					}

					maxIndex += bangles[i].meshes[j].vertexCount;
				}
			}
			File.WriteAllText(filePath, obj.ToString());
		}
		public void Dispose()
		{
			file.Dispose();
		}
	}
}