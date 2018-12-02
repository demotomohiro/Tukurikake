#version 450

layout(binding = 0) buffer layoutName {
  vec2 particles[];
};

const float radius = 0.05;

void main()
{
  vec2 pos = particles[gl_VertexID/3u];
  uint vecID = gl_VertexID % 3u;
  vec2 vpos;
  if(vecID == 0) {
    vpos = vec2(0.0, 0.0);
  }else if(vecID == 1) {
    vpos = vec2(radius, 0.0);
  }else {
    vpos = vec2(0.0, radius);
  }

  vec2 p = pos + vpos;
  p = p - vec2(0.5);
  gl_Position = vec4(p, 0.0, 1.0);
}
