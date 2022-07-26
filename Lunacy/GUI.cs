using ImGuiNET;

namespace Lunacy
{
	public class GUI
	{
		ImGuiController controller;
		Window wnd;

		Entity selectedEntity = null;

		public static int framebuffer;
		private static int framebufferTexture;

		public GUI(Window wnd)
		{
			controller = new ImGuiController(wnd.ClientSize.X, wnd.ClientSize.Y);
			this.wnd = wnd;
		}

		public static void InitializeFramebuffer()
		{
			framebuffer = GL.GenFramebuffer();
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);
			framebufferTexture = GL.GenTexture();
			GL.BindTexture(TextureTarget.Texture2D, framebufferTexture);
			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, 1280, 720, 0, PixelFormat.Rgb, PixelType.UnsignedShort, IntPtr.Zero);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
			GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, framebufferTexture, 0);

			if(GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
			{
				throw new Exception("Framebuffer incomplete");
			}
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
		}

		public void FrameBegin(double delta)
		{
			controller.Update(wnd, (float)delta);
		}

		public void ShowRegionsWindow()
		{
			ImGui.Begin("Regions", ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.AlwaysVerticalScrollbar);
			foreach(KeyValuePair<string, List<Entity>> mobys in EntityManager.Singleton.MobyHandles)
			{
				if(ImGui.CollapsingHeader(mobys.Key))
				{
					for(int i = 0; i < mobys.Value.Count; i++)
					{
						if((Camera.transform.Position + mobys.Value[i].transform.Position).LengthSquared < 0.01f) Console.WriteLine(mobys.Value[i].name);
						
						ImGui.PushID($"{mobys.Key}:{i}:{mobys.Value[i].name}");
						if(ImGui.Button(mobys.Value[i].name))
						{
							//if((mobys.Value[i].transform.Position).LengthSquared)
							Camera.transform.Position = -mobys.Value[i].transform.Position;
							selectedEntity = mobys.Value[i];
						}
						ImGui.PopID();
					}
				}
			}
			ImGui.End();

			ImGui.Begin("Zones", ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.AlwaysVerticalScrollbar);
			for(int j = 0; j < EntityManager.Singleton.TieInstances.Count; j++)
			{
				if(ImGui.CollapsingHeader($"Zone {j}"))
				{
					for(int i = 0; i < EntityManager.Singleton.TieInstances[j].Count; i++)
					{
						ImGui.PushID($"{j}:{i}:{EntityManager.Singleton.TieInstances[j][i].name}");
						if(ImGui.Button(EntityManager.Singleton.TieInstances[j][i].name))
						{
							Camera.transform.Position = -EntityManager.Singleton.TieInstances[j][i].transform.Position;
							selectedEntity = EntityManager.Singleton.TieInstances[j][i];
						}
						ImGui.PopID();
					}
				}
			}

			if(selectedEntity != null)
			{
				ShowEntityInfo();
			}
		}

		public void Tick()
		{
			Vector2 mouse = wnd.MouseState.Position;
			Vector3 viewport = new Vector3(
				(2 * mouse.X) / wnd.ClientSize.X - 1,
				1 - (2 * mouse.Y) / wnd.ClientSize.Y,
				1
			);
			Vector4 homogeneousClip = new Vector4(viewport.X, viewport.Y, -1, 1);
			Vector4 eye = Matrix4.Invert(Matrix4.Transpose(Camera.ViewToClip)) * homogeneousClip;
			eye.Z = -1;
			eye.W = 0;
			Vector3 world = (Matrix4.Invert(Matrix4.Transpose(Camera.WorldToView)) * eye).Xyz;
			world.Normalize();
			string entityNames = string.Empty;
			for(int i = 0; i < EntityManager.Singleton.MobyHandles.Count; i++)
			{
				for(int j = 0; j < EntityManager.Singleton.MobyHandles.ElementAt(i).Value.Count; j++)
				{
					if(EntityManager.Singleton.MobyHandles.ElementAt(i).Value[j].IntersectsRay(world, -Camera.transform.Position))
					{
						entityNames += $"{EntityManager.Singleton.MobyHandles.ElementAt(i).Value[j].name}\n";
					}
				}
			}
			for(int i = 0; i < EntityManager.Singleton.TieInstances.Count; i++)
			{
				for(int j = 0; j < EntityManager.Singleton.TieInstances[i].Count; j++)
				{
					if(EntityManager.Singleton.TieInstances[i][j].IntersectsRay(world, -Camera.transform.Position))
					{
						entityNames += $"{EntityManager.Singleton.TieInstances[i][j].name}\n";
					}
				}
			}
			ImGui.SetTooltip(entityNames);
			
		}

		private void ShowEntityInfo()
		{
			ImGui.Begin($"{selectedEntity.name} Properties");
			System.Numerics.Vector3 position = Utils.ToNumericsVector3(selectedEntity.transform.Position);
			ImGui.InputFloat3("Position: ", ref position);
			selectedEntity.SetPosition(Utils.ToOpenTKVector3(position));
			ImGui.End();
		}

		public void FrameEnd()
		{
			controller.Render();
		}
	}
}