using System.Numerics;
using System.Text;
using Vector3 = System.Numerics.Vector3;
using Quaternion = System.Numerics.Quaternion;

namespace LibLunacy
{
	public static class Utils
	{
		public static Vector3 ToNumerics(this in OpenTK.Mathematics.Vector3 input)
		{
			return new Vector3(input.X, input.Y, input.Z);
		}
		public static OpenTK.Mathematics.Vector3 ToOpenTK(this in Vector3 input)
		{
			return new OpenTK.Mathematics.Vector3(input.X, input.Y, input.Z);
		}

		public static System.Numerics.Quaternion ToNumerics(this in OpenTK.Mathematics.Quaternion input)
		{
			return new Quaternion(input.X, input.Y, input.Z, input.W);
		}

		public static OpenTK.Mathematics.Quaternion ToOpenTK(this in Quaternion input)
		{
			return new OpenTK.Mathematics.Quaternion(input.X, input.Y, input.Z, input.W);
        }

        public static Matrix4x4 ToNumerics(this in Matrix4 input)
        {
            return new Matrix4x4(
                input.M11, input.M12, input.M13, input.M14,
                input.M21, input.M22, input.M23, input.M24,
                input.M31, input.M32, input.M33, input.M34,
                input.M41, input.M42, input.M43, input.M44
            );
        }

        public static Matrix4 ToOpenTK(this in Matrix4x4 input)
		{
			return new OpenTK.Mathematics.Matrix4(
				input.M11, input.M12, input.M13, input.M14, 
				input.M21, input.M22, input.M23, input.M24, 
				input.M31, input.M32, input.M33, input.M34, 
				input.M41, input.M42, input.M43, input.M44
			);
		}

		public static string ToString(in object? obj)
		{
			if (obj is null)
				return "null";

			var fields = obj.GetType().GetFields();
			var sb = new StringBuilder();
			sb.AppendLine($"{obj.GetType().Name} {{");
			foreach ( var field in fields )
			{
				var val = field.GetValue(obj);
				sb.AppendLine($"\t{field.FieldType.Name} {field.Name}: {val};");
			}
			sb.AppendLine("}");
			return sb.ToString();
		}

		public static void DecomposeMatrix4(this in Matrix4 matrix, out Vector3 pos, out Quaternion rot, out Vector3 scale)
		{
			pos = matrix.ExtractTranslation().ToNumerics();
			rot = matrix.ExtractRotation().ToNumerics();
			scale = matrix.ExtractScale().ToNumerics();
		}
	}
}