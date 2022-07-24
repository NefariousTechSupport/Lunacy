namespace LibLunacy
{
	public static class Utils
	{
		public static System.Numerics.Vector3 ToNumericsVector3(OpenTK.Mathematics.Vector3 input)
		{
			return new System.Numerics.Vector3(input.X, input.Y, input.Z);
		}
		public static OpenTK.Mathematics.Vector3 ToOpenTKVector3(System.Numerics.Vector3 input)
		{
			return new OpenTK.Mathematics.Vector3(input.X, input.Y, input.Z);
		}

	}
}