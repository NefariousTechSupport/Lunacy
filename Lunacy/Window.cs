namespace Lunacy
{
	public class Window : GameWindow
	{
		FileManager fm;
		AssetLoader al;

		DrawableListList drawable;
		Transform transform = new Transform();

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

			GL.UseProgram(programHandle);

			if(fm.isOld)
			{
				//drawable = new DrawableListList(al.mobys[4]);						//ToD apogee space station ratchet
				drawable = new DrawableListList(al.mobys[0xA3]);					//ToD apogee space station talwyn
			}
			else
			{
				drawable = new DrawableListList(al.mobys[0x04396A536A8F9378]);	//ACiT ratchet
			}
		}

		protected override void OnRenderFrame(FrameEventArgs args)
		{
			base.OnRenderFrame(args);

			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			drawable.Draw(transform);

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

			base.OnUpdateFrame(args);
		}
	}
}