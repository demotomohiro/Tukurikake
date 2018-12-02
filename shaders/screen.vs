#version 420

out vec2 uv;
const float aspect = 1920.0/1080.0;

void main()
{
	if(gl_VertexID == 0)
	{
    uv = vec2(-aspect, -1.0);
		gl_Position = vec4(-1.0, -1.0, 0.0, 1.0);
	}else if(gl_VertexID == 1)
	{
    uv = vec2(-aspect, 1.0);
		gl_Position = vec4(-1.0, 1.0, 0.0, 1.0);
	}else if(gl_VertexID == 2)
	{
    uv = vec2(aspect, -1.0);
		gl_Position = vec4(1.0, -1.0, 0.0, 1.0);
	}else
  {
    uv = vec2(aspect, 1.0);
    gl_Position = vec4(1.0, 1.0, 0.0, 1.0);
  }
}
