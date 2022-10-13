using ImGuiNET;

namespace Lunacy
{
	public class GUI
	{
		ImGuiController controller;
		Window wnd;

		Entity selectedEntity = null;

		bool raycast = false;

		public GUI(Window wnd)
		{
			controller = new ImGuiController(wnd.ClientSize.X, wnd.ClientSize.Y);
			this.wnd = wnd;
		}

		public void Resize()
		{
			controller.WindowResized(wnd.ClientSize.X, wnd.ClientSize.Y);
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
						ImGui.PushID($"{mobys.Key}:{i}:{mobys.Value[i].name}");
						if(ImGui.Button(mobys.Value[i].name))
						{
							Camera.transform.position = -mobys.Value[i].transform.position;
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
				if(ImGui.CollapsingHeader(EntityManager.Singleton.zones[j].name))
				{
					for(int i = 0; i < EntityManager.Singleton.TieInstances[j].Count; i++)
					{
						ImGui.PushID($"{j}:{i}:{EntityManager.Singleton.TieInstances[j][i].name}");
						if(ImGui.Button(EntityManager.Singleton.TieInstances[j][i].name))
						{
							Camera.transform.position = -EntityManager.Singleton.TieInstances[j][i].transform.position;
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
			if(wnd.KeyboardState.IsKeyPressed(Keys.P)) raycast = !raycast;

			if(raycast)
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
						if(EntityManager.Singleton.MobyHandles.ElementAt(i).Value[j].IntersectsRay(world, -Camera.transform.position))
						{
							entityNames += $"{EntityManager.Singleton.MobyHandles.ElementAt(i).Value[j].name}\n";
						}
					}
				}
				for(int i = 0; i < EntityManager.Singleton.TieInstances.Count; i++)
				{
					for(int j = 0; j < EntityManager.Singleton.TieInstances[i].Count; j++)
					{
						if(EntityManager.Singleton.TieInstances[i][j].IntersectsRay(world, -Camera.transform.position))
						{
							entityNames += $"{EntityManager.Singleton.TieInstances[i][j].name}\n";
						}
					}
				}
				ImGui.SetTooltip(entityNames);
			}			
		}

		public void KeyPress(int c)
		{
			controller.PressChar((char)c);
		}

		private float test;
		private string strtest = string.Empty;

		private void ShowEntityInfo()
		{
			ImGui.Begin($"{selectedEntity.name} Properties");
			bool posChanged = false;
			bool rotChanged = false;
			bool scaleChanged = false;
			System.Numerics.Vector3 position = Utils.ToNumericsVector3(selectedEntity.transform.position);
			System.Numerics.Vector3 rotation = Utils.ToNumericsVector3(selectedEntity.transform.eulerRotation * (180f / MathHelper.Pi));
			System.Numerics.Vector3 scale = Utils.ToNumericsVector3(selectedEntity.transform.scale);
			if(ImGui.InputFloat3("Position", ref position)) posChanged = true;
			if(ImGui.InputFloat3("Rotation", ref rotation)) rotChanged = true;
			if(ImGui.InputFloat3("Scale", ref scale)) scaleChanged = true;
			if(posChanged) selectedEntity.SetPosition(Utils.ToOpenTKVector3(position));
			if(rotChanged) selectedEntity.SetRotation(Utils.ToOpenTKVector3(rotation / (180f / MathHelper.Pi)));
			if(scaleChanged) selectedEntity.SetScale(Utils.ToOpenTKVector3(scale));
			ImGui.End();
			//ImGui.ShowDemoWindow();
		}

		public void FrameEnd()
		{
			controller.Render();
		}
	}
}