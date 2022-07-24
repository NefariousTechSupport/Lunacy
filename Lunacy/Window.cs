using ImGuiNET;

namespace Lunacy
{
	public class Window : GameWindow
	{
		FileManager fm;
		AssetLoader al;		// handles loading assets from files
		AssetManager am;	// handles converting assets to OpenGL
		Gameplay gp;

		ImGuiController guiController;

		public Vector2 freecamLocal;

		public Window(GameWindowSettings gws, NativeWindowSettings nws, string folderPath) : base(gws, nws)
		{
			LoadFolder(folderPath);
		}

		public void LoadFolder(string folderPath)
		{
			fm = new FileManager();
			fm.LoadFolder(folderPath);
			al = new AssetLoader(fm);
			al.LoadTextures();
			al.LoadShaders();
			al.LoadMobys();
		}

		protected override void OnLoad()
		{
			base.OnLoad();

			GL.ClearColor(0, 0, 0, 0);
			GL.Enable(EnableCap.DepthTest);
			GL.Enable(EnableCap.Texture2D);

			int programHandle = MaterialManager.LoadMaterial("stdv;ulitf", "shaders/stdv.glsl", "shaders/ulitf.glsl");

			Camera.CreatePerspective(MathHelper.PiOver2);

			guiController = new ImGuiController(ClientSize.X, ClientSize.Y);

			am = new AssetManager();
			am.Initialize(al);

			gp = new Gameplay(al);

			for(int i = 0; i < gp.regions.Length; i++)
			{
				KeyValuePair<ulong, Region.CMobyInstance>[] mobys = gp.regions[i].mobyInstances.ToArray();
				for(ulong j = 0; j < (ulong)mobys.Length; j++)
				{
					am.mobys[mobys[j].Value.moby.id].AddDrawCall(new Transform(
						new Vector3(mobys[j].Value.position.X, mobys[j].Value.position.Y, mobys[j].Value.position.Z),
						new Vector3(mobys[j].Value.rotation.X, mobys[j].Value.rotation.Y, mobys[j].Value.rotation.Z),
						Vector3.One * mobys[j].Value.scale
					));
				}
			}

			foreach(KeyValuePair<ulong, DrawableListList> moby in am.mobys)
			{
				moby.Value.ConsolidateDrawCalls();
			}

		}

		protected override void OnRenderFrame(FrameEventArgs args)
		{
			base.OnRenderFrame(args);

			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			guiController.Update(this, (float)args.Time);

			foreach(KeyValuePair<ulong, DrawableListList> moby in am.mobys)
			{
				moby.Value.Draw();
			}

			ImGui.Begin("Regions", ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.AlwaysVerticalScrollbar);
			for(int i = 0; i < gp.regions.Length; i++)
			{
				if(ImGui.CollapsingHeader(gp.regions[i].name, ImGuiTreeNodeFlags.None))
				{
					foreach(KeyValuePair<ulong, Region.CMobyInstance> moby in gp.regions[i].mobyInstances)
					{
						ImGui.Text(moby.Value.name);
					}
				}
			}
			ImGui.End();

			//ImGui.ShowDemoWindow();

			guiController.Render();

			SwapBuffers();
		}

		protected override void OnUpdateFrame(FrameEventArgs args)
		{
			if(KeyboardState.IsKeyDown(Keys.Escape)) Close();

			float moveSpeed = 5;
			float sensitivity = 0.01f;

			if(KeyboardState.IsKeyDown(Keys.LeftShift)) moveSpeed *= 10;

			if(KeyboardState.IsKeyDown(Keys.W)) Camera.transform.Position += Camera.transform.Forward * (float)args.Time * moveSpeed;
			if(KeyboardState.IsKeyDown(Keys.A)) Camera.transform.Position += Camera.transform.Right * (float)args.Time * moveSpeed;
			if(KeyboardState.IsKeyDown(Keys.S)) Camera.transform.Position -= Camera.transform.Forward * (float)args.Time * moveSpeed;
			if(KeyboardState.IsKeyDown(Keys.D)) Camera.transform.Position -= Camera.transform.Right * (float)args.Time * moveSpeed;

			freecamLocal += MouseState.Delta.Yx * sensitivity;

			freecamLocal.X = MathHelper.Clamp(freecamLocal.X, -MathHelper.PiOver2 + 0.0001f, MathHelper.PiOver2 - 0.0001f);

			Camera.transform.Rotation = Quaternion.FromAxisAngle(Vector3.UnitX, freecamLocal.X) * Quaternion.FromAxisAngle(Vector3.UnitY, freecamLocal.Y);

			Title = $"Lunacy Level Editor | {1 / args.Time}";

			base.OnUpdateFrame(args);
		}
	}
}