namespace Lunacy
{
	public class Texture
	{
		public int textureId;

		public unsafe Texture(CTexture ctex)
		{
			textureId = GL.GenTexture();

			GL.ActiveTexture(TextureUnit.Texture0);
			GL.BindTexture(TextureTarget.Texture2D, textureId);

			fixed (byte* b = ctex.data)
			{
				uint offset = 0;
				for(int i = 0; i < ctex.mipmapCount; i++)
				{
					if(ctex.format == CTexture.TexFormat.DXT1)
					{
						int size = (Math.Max( 1, ((ctex.width / (int)Math.Pow(2, i))+3)/4) * Math.Max(1, ((ctex.height / (int)Math.Pow(2, i)) +3)/4)) * 8;
						GL.CompressedTexImage2D(TextureTarget.Texture2D, i, InternalFormat.CompressedRgbS3tcDxt1Ext, ctex.width, ctex.height, 0, size, (IntPtr)(b + offset));
						offset += (uint)size;
					}
					else if (ctex.format == CTexture.TexFormat.DXT3)
					{
						int size = (Math.Max( 1, ((ctex.width / (int)Math.Pow(2, i))+3)/4) * Math.Max(1, ((ctex.height / (int)Math.Pow(2, i)) +3)/4)) * 16;
						GL.CompressedTexImage2D(TextureTarget.Texture2D, i, InternalFormat.CompressedRgbaS3tcDxt3Ext, ctex.width, ctex.height, 0, size, (IntPtr)(b + offset));
						offset += (uint)size;
					}
					else if (ctex.format == CTexture.TexFormat.DXT5)
					{
						int size = (Math.Max( 1, ((ctex.width / (int)Math.Pow(2, i))+3)/4) * Math.Max(1, ((ctex.height / (int)Math.Pow(2, i)) +3)/4)) * 16;
						GL.CompressedTexImage2D(TextureTarget.Texture2D, i, InternalFormat.CompressedRgbaS3tcDxt5Ext, ctex.width, ctex.height, 0, size, (IntPtr)(b + offset));
						offset += (uint)size;
					}
				}					
			}

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);

			GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

			GL.BindTexture(TextureTarget.Texture2D, 0);
		}

		public void Use()
		{
			GL.ActiveTexture(TextureUnit.Texture0);
			GL.BindTexture(TextureTarget.Texture2D, textureId);
		}
	}
}