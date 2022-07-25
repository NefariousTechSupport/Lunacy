using System.Numerics;

namespace LibLunacy
{
	public class CTie
	{
		[FileStructure(0x40)]
		public struct TieMesh
		{
			[FileOffset(0x00)] public uint indexIndex;
			[FileOffset(0x04)] public ushort vertexIndex;
			[FileOffset(0x08)] public ushort vertexCount;
			[FileOffset(0x12)] public ushort indexCount;
			[FileOffset(0x28)] public ushort oldShaderIndex;
			[FileOffset(0x2A)] public byte newShaderIndex;

			public float[] vPositions;
			public float[] vTexCoords;
			public uint[] indices;
			public CShader shader;
		}
		[FileStructure(0x80)]
		public struct Tie
		{
			[FileOffset(0x00), Reference("MetadataCount")] public TieMesh[] meshes;
			[FileOffset(0x0C)] public uint metadataCount;	//Note: this could be a ushort starting from 0x0E
			[FileOffset(0x14)] public uint vertexBufferStart;
			[FileOffset(0x18)] public uint vertexBufferSize;
			[FileOffset(0x20)] public Vector3 scale;
			[FileOffset(0x64), Reference] public string name;	//nullptr on old engine
			[FileOffset(0x68)] public ulong tuid;				//0 on old engine

			public uint MetadataCount => metadataCount;
		}

		public TieMesh[] meshes;
		public string name;
		public IGFile file;
		public ulong id;		//Either tuid or index depending on game
		private Vector3 scale;

		public CTie(IGFile file, AssetLoader al, uint index = 0)
		{
			this.file = file;
			IGFile.SectionHeader section = file.QuerySection(0x3400);

			Console.WriteLine($"Loading Tie {(section.offset + index * 0x80).ToString("X08")}");
			file.sh.Seek(section.offset + index * 0x80);
			Tie tie = FileUtils.ReadStructure<Tie>(file.sh);

			meshes = tie.meshes;
			scale = tie.scale;

			if(al.fm.isOld)
			{
				name = $"Tie_{index.ToString("X04")}";
				id = section.offset + index * 0x80;
			}
			else
			{
		    	name = tie.name;
				id = tie.tuid;
			}

			InitializeBuffers(al, ref tie);
			LoadDependancies(al);
		}

		private void InitializeBuffers(AssetLoader al, ref Tie tie)
		{
			IGFile.SectionHeader vertexSection;
			IGFile.SectionHeader indexSection;

			IGFile geometryFile;
			StreamHelper vertexStream;

			if(al.fm.isOld)
			{
				geometryFile = al.fm.igfiles["vertices.dat"];
				vertexSection = geometryFile.QuerySection(0x9000);
				indexSection = geometryFile.QuerySection(0x9100);
			}
			else
			{
				geometryFile = file;
				vertexSection = file.QuerySection(0x3000);
				indexSection = file.QuerySection(0x3200);
			}

			geometryFile.sh.Seek(vertexSection.offset + tie.vertexBufferStart);
			vertexStream = new StreamHelper(new MemoryStream(geometryFile.sh.ReadBytes(tie.vertexBufferSize)), file.sh._endianness);

			for(int i = 0; i < meshes.Length; i++)
			{
				meshes[i].indices = new uint[meshes[i].indexCount];
				geometryFile.sh.Seek(indexSection.offset + meshes[i].indexIndex * 2);
				for(int j = 0; j < meshes[i].indexCount; j++)
				{
					meshes[i].indices[j] = geometryFile.sh.ReadUInt16();
				}

				meshes[i].vPositions = new float[meshes[i].vertexCount * 3];
				meshes[i].vTexCoords = new float[meshes[i].vertexCount * 2];
				for(int j = 0; j < meshes[i].vertexCount; j++)
				{
					vertexStream.Seek((meshes[i].vertexIndex + j) * 0x14 + 0x00);
					meshes[i].vPositions[j * 3 + 0] = vertexStream.ReadInt16() * scale.X;
					meshes[i].vPositions[j * 3 + 1] = vertexStream.ReadInt16() * scale.Y;
					meshes[i].vPositions[j * 3 + 2] = vertexStream.ReadInt16() * scale.Z;

					vertexStream.Seek((meshes[i].vertexIndex + j) * 0x14 + 0x08);
					meshes[i].vTexCoords[j * 2 + 0] = (float)vertexStream.ReadHalf();
					meshes[i].vTexCoords[j * 2 + 1] = (float)vertexStream.ReadHalf();
				}
			}
		}
		private void LoadDependancies(AssetLoader al)
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
			for(int i = 0; i < meshes.Length; i++)
			{
				if(al.fm.isOld)
				{
					meshes[i].shader = al.shaders[meshes[i].oldShaderIndex];
				}
				else
				{
					file.sh.Seek(shaderSection.offset + meshes[i].newShaderIndex * 8);
					meshes[i].shader = al.shaders[file.sh.ReadUInt64()];
				}
			}
		}
		public void ExportToObj(string filePath)
		{
			uint maxIndex = 0;

			StringBuilder obj = new StringBuilder();

			obj.Append($"mtllib unused.mtl\n");
			for(int i = 0; i < meshes.Length; i++)
			{
				obj.Append($"o Mesh_{i}\n");
				Console.WriteLine($"Submesh {i+1}/{meshes.Length}");

				for(int k = 0; k < meshes[i].vertexCount; k++)
				{
					obj.Append($"v {meshes[i].vPositions[k * 3].ToString("F8")} {meshes[i].vPositions[k * 3 + 1].ToString("F8")} {meshes[i].vPositions[k * 3 + 2].ToString("F8")}\n");
					obj.Append($"vt {meshes[i].vTexCoords[k * 2].ToString("F8")} {meshes[i].vTexCoords[k * 2 + 1].ToString("F8")}\n");
					//obj.Append($"vn {meshes[i].vNormals[k * 3].ToString("F8")} {meshes[i].vNormals[k * 3 + 1].ToString("F8")} {meshes[i].vNormals[k * 3 + 2].ToString("F8")}\n");
				}

				obj.Append($"usemtl Shader_{meshes[i].oldShaderIndex}\n");

				for(int k = 0; k < meshes[i].indexCount; k += 3)
				{
					string i1 = (meshes[i].indices[k + 0] + maxIndex + 1).ToString();
					string i2 = (meshes[i].indices[k + 1] + maxIndex + 1).ToString();
					string i3 = (meshes[i].indices[k + 2] + maxIndex + 1).ToString();
					//obj.Append($"f {i1}/{i1}/{i1} {i2}/{i2}/{i2} {i3}/{i3}/{i3}\n");
					obj.Append($"f {i1}/{i1} {i2}/{i2} {i3}/{i3}\n");
				}

				maxIndex += meshes[i].vertexCount;
			}
			File.WriteAllText(filePath, obj.ToString());
		}
	}
}