namespace Lunacy
{
	//Buffers are split up due to how not all vertex attributes are currently known.
	//Creating one single buffer structure where the data is interweaved could lead to issues with excess memory usage for meshes where those extra vertex attributes aren't in use.
	//One example of such is blending, only some moby meshes have this and it would be a waste of memory to store this for ties, zone meshes, and shrubs.
	public class Drawable
	{
		private List<Transform> transforms = new List<Transform>();
		public Drawable(){}
		public Drawable(ref CMoby.MobyMesh mesh)
		{
			Prepare();
			SetVertexPositions(mesh.vPositions);
			SetVertexTexCoords(mesh.vTexCoords);
			SetIndices(mesh.indices);
			Texture? tex = (mesh.shader.albedo == null ? null : new Texture(mesh.shader.albedo));
			SetMaterial(new Material(MaterialManager.materials["stdv;ulitf"], tex));
		}

		int VwBO;
		int VpBO;
		int VtcBO;
		int VAO;
		int EBO;
		int indexCount;
		Material material;

		public void Prepare()
		{
			VAO = GL.GenVertexArray();
			EBO = GL.GenBuffer();
		}

		public void SetIndices(uint[] indices)
		{
			GL.BindVertexArray(VAO);
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, EBO);
			GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);
			indexCount = indices.Length;
		}

		//It goes:
		//	0: vertex positions  (3 floats)
		//	1: vertex tex coords (2 floats)
		public void SetVertexPositions(float[] vpositions)
		{
			VpBO = GL.GenBuffer();
			GL.BindBuffer(BufferTarget.ArrayBuffer, VpBO);
			GL.BufferData(BufferTarget.ArrayBuffer, vpositions.Length * sizeof(float), vpositions, BufferUsageHint.StaticDraw);

			GL.BindVertexArray(VAO);
			GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
			GL.EnableVertexAttribArray(0);
		}
		public void SetVertexTexCoords(float[] vtexcoords)
		{
			VtcBO = GL.GenBuffer();
			GL.BindBuffer(BufferTarget.ArrayBuffer, VtcBO);
			GL.BufferData(BufferTarget.ArrayBuffer, vtexcoords.Length * sizeof(float), vtexcoords, BufferUsageHint.StaticDraw);

			GL.BindVertexArray(VAO);
			GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
			GL.EnableVertexAttribArray(1);
		}
		public void SetMaterial(Material mat)
		{
			material = mat;
		}

		public void AddDrawCall(Transform transform)
		{
			transforms.Add(transform);
		}

		public void ConsolidateDrawCalls()
		{
			if(VwBO != 0) GL.DeleteBuffer(VwBO);

			Matrix4[] transformMatrices = new Matrix4[transforms.Count];
			for(int i = 0; i < transformMatrices.Length; i++)
			{
				transformMatrices[i] = transforms[i].GetLocalToWorldMatrix();
			}

			VwBO = GL.GenBuffer();
			GL.BindBuffer(BufferTarget.ArrayBuffer, VwBO);
			GL.BufferData(BufferTarget.ArrayBuffer, transformMatrices.Length * sizeof(float) * 16, transformMatrices, BufferUsageHint.StaticDraw);	//Note: this should be edited in the future so mobys can be moved
			
			GL.BindVertexArray(VAO);
			GL.VertexAttribPointer(4, 4, VertexAttribPointerType.Float, false, sizeof(float) * 16, 0);
			GL.VertexAttribPointer(5, 4, VertexAttribPointerType.Float, false, sizeof(float) * 16, sizeof(float) * 4);
			GL.VertexAttribPointer(6, 4, VertexAttribPointerType.Float, false, sizeof(float) * 16, sizeof(float) * 4);
			GL.VertexAttribPointer(7, 4, VertexAttribPointerType.Float, false, sizeof(float) * 16, sizeof(float) * 4);
			GL.VertexAttribDivisor(4, 1);
			GL.VertexAttribDivisor(5, 1);
			GL.VertexAttribDivisor(6, 1);
			GL.VertexAttribDivisor(7, 1);
			GL.EnableVertexAttribArray(4);
			GL.EnableVertexAttribArray(5);
			GL.EnableVertexAttribArray(6);
			GL.EnableVertexAttribArray(7);
		}

		public void Draw()
		{
			material.Use();
			for(int i = 0; i < transforms.Count; i++)
			{
				material.SetMatrix4x4("world", transforms[i].GetLocalToWorldMatrix() * Camera.WorldToView * Camera.ViewToClip);

				GL.BindVertexArray(VAO);
				GL.DrawElements(PrimitiveType.Triangles, indexCount, DrawElementsType.UnsignedInt, 0);
			}
		}
	}

	public class DrawableList : List<Drawable>
	{
		public DrawableList(ref CMoby.Bangle bangle)
		{
			this.Capacity = (int)bangle.count;
			for(int i = 0; i < bangle.count; i++)
			{
				this.Add(new Drawable(ref bangle.meshes[i]));
			}
		}
		public void AddDrawCall(Transform transform)
		{
			for(int i = 0; i < Count; i++)
			{
				this[i].AddDrawCall(transform);
			}
		}
		public void Draw()
		{
			for(int i = 0; i < Count; i++)
			{
				this[i].Draw();
			}
		}
	}

	public class DrawableListList : List<DrawableList>
	{
		public DrawableListList(CMoby moby)
		{
			this.Capacity = (int)moby.bangles.Length;
			for(int i = 0; i < moby.bangles.Length; i++)
			{
				this.Add(new DrawableList(ref moby.bangles[i]));
			}
		}
		public void AddDrawCall(Transform transform)
		{
			for(int i = 0; i < Count; i++)
			{
				this[i].AddDrawCall(transform);
			}
		}
		public void Draw()
		{
			for(int i = 0; i < Count; i++)
			{
				this[i].Draw();
			}
		}
	}
}