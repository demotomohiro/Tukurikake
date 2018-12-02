#version 450

uniform uint nframe;

layout(local_size_x = $CSLocalSize) in;
layout(binding = 0) buffer layoutName {
  vec2 particles[];
};

uint rand(uint x) {
  uint x0 = x * 1664525;
  x0 = x0 ^ (x0 >> 17);
  x0 = x0 * 1664525;
  x0 = x0 ^ (x0 >> 13);
  x0 = x0 * 1664525;
  return x0;
}

float uintToFloat(uint src) {
    return uintBitsToFloat(0x3f800000u | (src & 0x7fffffu))-1.0;
}

float randf(uint src) {
  return uintToFloat(rand(src));
}

void main(){
  uint id = gl_GlobalInvocationID.x;
  if(nframe == 0) {
    vec2 rndv = vec2(randf(id), randf(id+100000));
    particles[id] = rndv;
    return;
  }
}
