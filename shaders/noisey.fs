#version 420

in vec2 uv;
out vec4 color;

uniform float time;

void main()
{
  if(dot(uv, uv) < 0.9)
    discard;
	color = vec4(abs(uv), time*0.01, 0.0);
}
