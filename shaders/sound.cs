#version 450

layout(local_size_x = $soundCSLocalSize) in;
layout(binding = 0) buffer layoutName {
  vec2 samples[];
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

float rsin(float x) {
  float r = 0.0;
  for(int i=0; i<7; ++i) {
    r = sin(r + x);
  }
  return r;
}

#define PI	3.1415926

void main(){
  uint id = gl_GlobalInvocationID.x;
  if(id >= $soundNumSamples)
    return;

  float t = float(id)/$soundSampleRate;

  float freq = exp2(randf(uint(t*4.0)))*0.5*200.0;
//  float v = sin(2.0 * PI * t * 400.0)*0.25;
  float v = fract(2.0 * PI * t * freq + rsin(2.0 * PI * t * 1.5)*200.0)*0.25;
  v = max(rsin(2.0*PI*t*4.0), 0.0)*v;
  samples[id] = (vec2(v) + vec2(1.0)) * 0.5;
}
