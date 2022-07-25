using ImGuiNET;

namespace Lunacy
{
	public class GUI
	{
		ImGuiController controller;
		Window wnd;

		Entity selectedEntity = null;

		public GUI(Window wnd)
		{
			controller = new ImGuiController(wnd.ClientSize.X, wnd.ClientSize.Y);
			this.wnd = wnd;
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