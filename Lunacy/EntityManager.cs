namespace Lunacy
{
	public class EntityManager
	{
		static Lazy<EntityManager> lazy = new Lazy<EntityManager>(() => new EntityManager());

		public static EntityManager Singleton => lazy.Value;

		public Dictionary<string, List<Entity>> MobyHandles = new Dictionary<string, List<Entity>>();
		public List<List<Entity>> TieInstances = new List<List<Entity>>();
		public List<List<Entity>> TFrags = new List<List<Entity>>();

		List<Entity> mobys = new List<Entity>();

		public void LoadGameplay(Gameplay gp)
		{
			for(int i = 0; i < gp.regions.Length; i++)
			{
				MobyHandles.Add(gp.regions[i].name, new List<Entity>());
				KeyValuePair<ulong, Region.CMobyInstance>[] mobys = gp.regions[i].mobyInstances.ToArray();
				for(ulong j = 0; j < (ulong)mobys.Length; j++)
				{
					MobyHandles[gp.regions[i].name].Add(new Entity(mobys[j].Value));
				}
			}

			AssetManager.Singleton.ConsolidateMobys();

			for(int i = 0; i < gp.zones.Length; i++)
			{
				TieInstances.Add(new List<Entity>());
				KeyValuePair<ulong, Zone.CTieInstance>[] ties = gp.zones[i].tieInstances.ToArray();
				for(ulong j = 0; j < (ulong)ties.Length; j++)
				{
					TieInstances[i].Add(new Entity(ties[j].Value));
				}
				TFrags.Add(new List<Entity>());
				for(uint j = 0; j < gp.zones[i].tfrags.Length; j++)
				{
					TFrags[i].Add(new Entity(gp.zones[i].tfrags[j]));
				}
			}

			AssetManager.Singleton.ConsolidateTies();
		}

		private void ReallocEntities()
		{
			foreach(KeyValuePair<string, List<Entity>> region in MobyHandles)
			{
				mobys.AddRange(region.Value);
			}
		}

		public void Render()
		{
			/*if(MobyHandles.Sum(x => x.Value.Count) != mobys.Count)
			{
				ReallocEntities();
			}
			mobys = mobys.OrderByDescending(x => (x.transform.Position + Camera.transform.Position).LengthSquared).ToList();
			for(int i = 0; i < mobys.Count; i++)
			{
				mobys[i].Draw();
			}*/
			KeyValuePair<ulong, DrawableListList>[] mobys = AssetManager.Singleton.mobys.ToArray();
			for(int i = 0; i < mobys.Length; i++)
			{
				mobys[i].Value.Draw();

			}
			KeyValuePair<ulong, DrawableList>[] ties = AssetManager.Singleton.ties.ToArray();
			for(int i = 0; i < ties.Length; i++)
			{
				ties[i].Value.Draw();
			}
			foreach(List<Entity> tfrag in TFrags)
			{
				for(int i = 0; i < tfrag.Count; i++)
				{
					tfrag[i].Draw();
				}
			}
		}
	}

	public class Entity
	{
		public object instance;					//Is either a Region.CMobyInstance or a TieInstance depending on if it's a moby or tie repsectively
		public object drawable;					//Is either a DrawableListList or a DrawableList depending on if it's a moby or tie respectively
		public int id;
		public string name = string.Empty;

		public Transform transform;

		//xyz is pos, w is radius
		public Vector4 boundingSphere;

		public Entity(Region.CMobyInstance mobyInstance)
		{
			instance = mobyInstance;
			drawable = AssetManager.Singleton.mobys[mobyInstance.moby.id];
			transform = new Transform(
						new Vector3(mobyInstance.position.X, mobyInstance.position.Y, mobyInstance.position.Z),
						new Vector3(mobyInstance.rotation.X, mobyInstance.rotation.Y, mobyInstance.rotation.Z),
						Vector3.One * mobyInstance.scale
					);
			name = mobyInstance.name;
			(drawable as DrawableListList).AddDrawCall(transform);
			boundingSphere = new Vector4(Utils.ToOpenTKVector3(mobyInstance.moby.boundingSpherePosition) + transform.Position, mobyInstance.moby.boundingSphereRadius * mobyInstance.scale);
		}
		public Entity(Zone.CTieInstance tieInstance)
		{
			instance = tieInstance;
			drawable = AssetManager.Singleton.ties[tieInstance.tie.id];
			transform = new Transform(Utils.ToOpenTKMatrix4(tieInstance.transformation));
			name = tieInstance.name;
			(drawable as DrawableList).AddDrawCall(transform);
			boundingSphere = new Vector4(Utils.ToOpenTKVector3(tieInstance.boundingPosition), tieInstance.boundingRadius);
		}
		public Entity(Zone.NewTFrag tfrag)
		{
			instance = tfrag;
			drawable = new Drawable(ref tfrag);
			name = "hi";
			transform = new Transform(Utils.ToOpenTKMatrix4(tfrag.transformation));
		}

		public void SetPosition(Vector3 position)
		{
			transform.Position = position;
			if(drawable is DrawableListList dll) dll.UpdateTransform(transform);
			else if(drawable is DrawableList dl) dl.UpdateTransform(transform);
		}
		public void Draw()
		{
			if(drawable is DrawableListList dll) dll.Draw();
			else if(drawable is DrawableList dl) dl.Draw();
			else if(drawable is Drawable d) d.Draw(transform);
		}
		public bool IntersectsRay(Vector3 dir, Vector3 position)
		{
			Vector3 m = position - boundingSphere.Xyz;
			float b = Vector3.Dot(m, dir);
			float c = Vector3.Dot(m, m) - boundingSphere.W * boundingSphere.W;

			if(c > 0 && b > 0) return false;

			float discriminant = b*b - c;

			return discriminant >= 0;
		}
	}
}