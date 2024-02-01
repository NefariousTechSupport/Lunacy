using ImGuiNET;
using System.Numerics;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;
using Vector4 = System.Numerics.Vector4;
using Matrix4 = OpenTK.Mathematics.Matrix4;

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

			if(true)
			{
				RenderInfoOverlay();
			}

			if(selectedEntity != null)
			{
				ShowEntityInfo();
			}
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
			if(ImGui.Begin("Stats", windowFlags))
			{
				ImGui.Text("Camera info");
				ImGui.Separator();
				ImGui.Text($"Pos: {Camera.transform.position}");
				ImGui.Text($"Rot: {Camera.transform.eulerRotation}");
				ImGui.Separator();
				ImGui.Text("Statistics");
				ImGui.Separator();
				ImGui.Text("Framerate: ");
				ImGui.SameLine();
				ImGui.TextColored(Window.framerate > 30 ? new Vector4(0.15f, 1f, 0.15f, 1f) : new Vector4(1f, 0.15f, 0.15f, 1f), $"{Math.Round(Window.framerate)}");
			}
			ImGui.End();
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

		private float test;
		private string strtest = string.Empty;

		private void ShowEntityInfo()
		{
			ImGui.Begin($"{selectedEntity.name} Properties");
			bool posChanged = false;
			bool rotChanged = false;
			bool scaleChanged = false;
			System.Numerics.Vector3 position = Utils.ToNumerics(selectedEntity.transform.position);
			System.Numerics.Vector3 rotation = Utils.ToNumerics(selectedEntity.transform.eulerRotation * (180f / MathHelper.Pi));
			System.Numerics.Vector3 scale = Utils.ToNumerics(selectedEntity.transform.scale);
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