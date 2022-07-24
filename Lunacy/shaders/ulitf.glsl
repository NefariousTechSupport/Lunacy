#version 440 core

out vec4 color;

in vec2 UVs;

uniform sampler2D albedo;
uniform bool useTexture;

void main()
{
	if(useTexture)
	{
		color = texture(albedo, UVs);
	}
	else
	{
		color = vec4(1.0, 0.0, 1.0, 1.0);
	}
}