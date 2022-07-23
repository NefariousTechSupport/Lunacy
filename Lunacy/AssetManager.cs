namespace Lunacy
{
	//This class converts the games assets into things that OpenGL can deal with, along with caching them to cut down on memory and loads
	public class AssetManager
	{
		private static readonly Lazy<AssetManager> lazy = new Lazy<AssetManager>(() => new AssetManager());
		public static AssetManager Singleton => lazy.Value;

		public Dictionary<ulong, DrawableListList> mobys = new Dictionary<ulong, DrawableListList>();
		public Dictionary<uint, Texture> textures = new Dictionary<uint, Texture>();

		public void Initialize(AssetLoader al)
		{
			//TODO: cache textures and materials
			//foreach(KeyValuePair<uint, CTexture> ctex in al.textures)
			//{
			//	textures.Add(ctex.Key, new Texture(ctex.Value));
			//}
			foreach(KeyValuePair<ulong, CMoby> moby in al.mobys)
			{
				mobys.Add(moby.Key, new DrawableListList(moby.Value));
			}
		}
	}
}