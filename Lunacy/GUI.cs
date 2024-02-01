using ImGuiNET;
using System.Numerics;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;
using Vector4 = System.Numerics.Vector4;
using Matrix4 = OpenTK.Mathematics.Matrix4;
using System.Text.RegularExpressions;

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
			RenderRegionsExplorer();
			RenderZonesExplorer();

			if(true)
			{
				RenderInfoOverlay();
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

				OpenTK.Mathematics.Vector2 mouse = wnd.MouseState.Position;
				OpenTK.Mathematics.Vector3 viewport = new Vector3(
					(2 * mouse.X) / wnd.ClientSize.X - 1,
					1 - (2 * mouse.Y) / wnd.ClientSize.Y,
					1
				).ToOpenTK();
				OpenTK.Mathematics.Vector4 homogeneousClip = new(viewport.X, viewport.Y, -1, 1);
				OpenTK.Mathematics.Vector4 eye = Matrix4.Invert(Matrix4.Transpose(Camera.ViewToClip)) * homogeneousClip;
				eye.Z = -1;
				eye.W = 0;
				OpenTK.Mathematics.Vector3 world = (Matrix4.Invert(Matrix4.Transpose(Camera.WorldToView)) * eye).Xyz;
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

        string mobysSearchArgs = string.Empty;
		public void RenderRegionsExplorer()
		{
            ImGui.Begin("Regions", ImGuiWindowFlags.AlwaysVerticalScrollbar);
            ImGui.InputTextWithHint("Search", "blob_small, QWARK_NURSE, etc...", ref mobysSearchArgs, 0xFF);
            ImGui.Separator();
            foreach (KeyValuePair<string, List<Entity>> mobys in EntityManager.Singleton.MobyHandles)
            {
                bool hasMobys = false;
                if (ImGui.CollapsingHeader(mobys.Key))
                {
                    for (int i = 0; i < mobys.Value.Count; i++)
                    {
                        string mobyName = mobys.Value[i].name;
                        string searchRegex = string.Join("|", Regex.Escape(mobysSearchArgs.ToLower()).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
                        if (Regex.IsMatch(mobyName.ToLower(), searchRegex) || mobysSearchArgs.Length == 0)
                        {
                            hasMobys = true;
                        }
                        else continue;

                        ImGui.PushID($"{mobys.Key}:{i}:{mobys.Value[i].name}");
                        if (ImGui.Button(mobys.Value[i].name))
                        {
                            Camera.transform.position = -mobys.Value[i].transform.position;
                            selectedEntity = mobys.Value[i];
                        }
                        ImGui.PopID();
                    }
                }

                if(!hasMobys)
                {
                    ImGui.SetNextItemOpen(false, ImGuiCond.Once);
                }
            }
            ImGui.End();
        }

        string tiesSearchArgs = string.Empty;
        public void RenderZonesExplorer()
		{
            ImGui.Begin("Zones", ImGuiWindowFlags.AlwaysVerticalScrollbar);
            ImGui.InputTextWithHint("Search", "terrain, host, etc...", ref tiesSearchArgs, 0xFF);
            ImGui.Separator();
            for (int j = 0; j < EntityManager.Singleton.TieInstances.Count; j++)
            {
                bool hasTies = false;
                if (ImGui.CollapsingHeader(EntityManager.Singleton.zones[j].name))
                {
                    for (int i = 0; i < EntityManager.Singleton.TieInstances[j].Count; i++)
                    {
                        string tieName = EntityManager.Singleton.TieInstances[j][i].name;
                        string searchRegex = string.Join("|", Regex.Escape(tiesSearchArgs.ToLower()).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
                        if (Regex.IsMatch(tieName.ToLower(), searchRegex) || tiesSearchArgs.Length == 0)
                        {
                            hasTies = true;
                        }
                        else continue;

                        ImGui.PushID($"{j}:{i}:{tieName}");
                        if (ImGui.Button(tieName))
                        {
                            Camera.transform.position = -EntityManager.Singleton.TieInstances[j][i].transform.position;
                            selectedEntity = EntityManager.Singleton.TieInstances[j][i];
                        }
                        ImGui.PopID();
                    }
                }

                if (!hasTies)
                {
                    ImGui.SetNextItemOpen(false, ImGuiCond.Once);
                }
            }
            ImGui.End();
        }

        public void RenderInfoOverlay()
        {
            ImGuiIOPtr io = ImGui.GetIO();
            ImGuiWindowFlags windowFlags = ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoNav;

            float padding = 10f;
            ImGuiViewportPtr viewport = ImGui.GetMainViewport();
            Vector2 work_pos = viewport.WorkPos;
            Vector2 work_size = viewport.WorkSize;
            Vector2 win_pos, win_pos_pivot;
            win_pos.X = work_pos.X + padding;
            win_pos.Y = work_pos.Y + work_size.Y - padding;
            win_pos_pivot.X = 0;
            win_pos_pivot.Y = 1f;
            ImGui.SetNextWindowPos(win_pos, ImGuiCond.Always, win_pos_pivot);
            windowFlags |= ImGuiWindowFlags.NoMove;  // Locks the overlay;

            ImGui.SetNextWindowBgAlpha(0.4f);
            if (ImGui.Begin("Stats", windowFlags))
            {
                var ufragsCount = 0;
                for (int i = 0; i < EntityManager.Singleton.TFrags.Count - 1; i++)
                    ufragsCount += EntityManager.Singleton.TFrags[i].Count;
                var mobysCount = Window.al.mobys.Count;
                var tiesCount = 0;
                for (int i = 0; i < EntityManager.Singleton.TieInstances.Count - 1; i++)
                    tiesCount += EntityManager.Singleton.TieInstances[i].Count;

                ImGui.Text("Camera info");
                ImGui.Separator();
                ImGui.Text($"Pos: {Camera.transform.position}");
                ImGui.Text($"Rot: {Camera.transform.eulerRotation}");
                ImGui.Spacing();
                ImGui.Text("Statistics");
                ImGui.Separator();
                ImGui.Text("Framerate: ");
                ImGui.SameLine();
                ImGui.TextColored(Window.framerate > 25 ? new Vector4(0.15f, 1f, 0.15f, 1f) : new Vector4(1f, 0.15f, 0.15f, 1f), $"{Math.Round(Window.framerate)}FPS");
                ImGui.Text($"Regions: {EntityManager.Singleton.regions.Count}");
                ImGui.Text($"Zones: {EntityManager.Singleton.zones.Count}");
                ImGui.Text($"MobyHandles: {mobysCount}");
                ImGui.Text($"Ties: {tiesCount}");
                ImGui.Text($"UFrags: {ufragsCount}");
                ImGui.Text($"Shaders: {Window.al.shaders.Count}");
            }
            ImGui.End();
        }

		private void ShowEntityInfo()
		{
			ImGui.Begin($"{selectedEntity.name} Properties");
			bool posChanged = false;
			bool rotChanged = false;
			bool scaleChanged = false;
			System.Numerics.Vector3 position = selectedEntity.transform.position.ToNumerics();
			System.Numerics.Vector3 rotation = (selectedEntity.transform.eulerRotation * (180f / MathHelper.Pi)).ToNumerics();
			System.Numerics.Vector3 scale = selectedEntity.transform.scale.ToNumerics();
			if(ImGui.InputFloat3("Position", ref position)) posChanged = true;
			if(ImGui.InputFloat3("Rotation", ref rotation)) rotChanged = true;
			if(ImGui.InputFloat3("Scale", ref scale)) scaleChanged = true;
			if(posChanged) selectedEntity.SetPosition(Utils.ToOpenTK(position));
			if(rotChanged) selectedEntity.SetRotation(Utils.ToOpenTK(rotation / (180f / MathHelper.Pi)));
			if(scaleChanged) selectedEntity.SetScale(Utils.ToOpenTK(scale));
			ImGui.End();
			//ImGui.ShowDemoWindow();
		}

		public void FrameEnd()
		{
			controller.Render();
		}
	}
}