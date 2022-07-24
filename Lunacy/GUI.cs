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
						if(ImGui.Button(mobys.Value[i].name))
						{
							//Camera.transform.Position = -mobys.Value[i].transform.Position;
							selectedEntity = mobys.Value[i];
						}
					}
				}
			}
			ImGui.End();

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