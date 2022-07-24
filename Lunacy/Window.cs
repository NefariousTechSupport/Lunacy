using System.ComponentModel;

namespace Lunacy
{
	public class Window : GameWindow
	{
		FileManager fm;
		AssetLoader al;		// handles loading assets from files
		Gameplay gp;

		GUI gui;

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

			GL.ClearColor(0.1f, 0.1f, 0.1f, 0);
			GL.Enable(EnableCap.DepthTest);
			GL.Enable(EnableCap.Texture2D);
			//GL.Enable(EnableCap.StencilTest);
			GL.Enable(EnableCap.CullFace);
			//GL.Enable(EnableCap.Blend);
			//GL.BlendFuncSeparate(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha, BlendingFactorSrc.One, BlendingFactorDest.Zero);

			int programHandle = MaterialManager.LoadMaterial("stdv;ulitf", "shaders/stdv.glsl", "shaders/ulitf.glsl");

			Camera.CreatePerspective(MathHelper.PiOver2);

			gui = new GUI(this);

			AssetManager.Singleton.Initialize(al);

			gp = new Gameplay(al);

			EntityManager.Singleton.LoadGameplay(gp);
		}

		protected override void OnRenderFrame(FrameEventArgs args)
		{
			base.OnRenderFrame(args);

			//GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			gui.FrameBegin(args.Time);

			/*foreach(KeyValuePair<ulong, DrawableListList> moby in AssetManager.Singleton.mobys)
			{
				moby.Value.Draw();
			}*/

			EntityManager.Singleton.Render();

			gui.ShowRegionsWindow();

			gui.FrameEnd();

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

		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);
		}
	}
}