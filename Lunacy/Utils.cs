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

		public static OpenTK.Mathematics.Matrix4 ToOpenTKMatrix4(System.Numerics.Matrix4x4 input)
		{
			return new OpenTK.Mathematics.Matrix4(
				input.M11, input.M12, input.M13, input.M14, 
				input.M21, input.M22, input.M23, input.M24, 
				input.M31, input.M32, input.M33, input.M34, 
				input.M41, input.M42, input.M43, input.M44
			);
		}
	}
}