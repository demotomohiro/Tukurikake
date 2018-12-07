#version 420

in vec2 uv;
out vec4 color;

uniform float time;

const float PI = 3.1415926;

void umulExtended_(uint a, uint b, out uint hi, out uint lo) {
    const uint WHALF = 16u;
    const uint LOMASK = (1u<<WHALF)-1u;
    lo = a*b;               /* full low multiply */
    uint ahi = a>>WHALF;
    uint alo = a& LOMASK;
    uint bhi = b>>WHALF;
    uint blo = b& LOMASK;

    uint ahbl = ahi*blo;
    uint albh = alo*bhi;

    uint ahbl_albh = ((ahbl&LOMASK) + (albh&LOMASK));
    hi = ahi*bhi + (ahbl>>WHALF) +  (albh>>WHALF);
    hi += ahbl_albh >> WHALF; /* carry from the sum of lo(ahbl) + lo(albh) ) */
    /* carry from the sum with alo*blo */
    hi += ((lo >> WHALF) < (ahbl_albh&LOMASK)) ? 1u : 0u;
}

uvec2 philox4x32Bumpkey(uvec2 key) {
    uvec2 ret = key;
    ret.x += 0x9E3779B9u;
    ret.y += 0xBB67AE85u;
    return ret;
}

uvec4 philox4x32Round(uvec4 state, uvec2 key) {
    const uint M0 = 0xD2511F53u, M1 = 0xCD9E8D57u;
    uint hi0, lo0, hi1, lo1;
//    umulExtended(M0, state.x, hi0, lo0);
//    umulExtended(M1, state.z, hi1, lo1);
    umulExtended_(M0, state.x, hi0, lo0);
    umulExtended_(M1, state.z, hi1, lo1);

    return uvec4(
        hi1^state.y^key.x, lo1,
        hi0^state.w^key.y, lo0);
}

uvec4 philox4x32_7(uvec4 plain, uvec2 key) {
    uvec4 state = plain;
    uvec2 round_key = key;

    for(int i=0; i<7; ++i) {
        state = philox4x32Round(state, round_key);
        round_key = philox4x32Bumpkey(round_key);
    }

    return state;
}

float uintToFloat(uint src) {
    return uintBitsToFloat(0x3f800000u | (src & 0x7fffffu))-1.0;
}

vec4 uintToFloat(uvec4 src) {
    return vec4(uintToFloat(src.x), uintToFloat(src.y), uintToFloat(src.z), uintToFloat(src.w));
}


float skewF(float n)
{
    return (sqrt(n + 1.0) - 1.0)/n;
}

float unskewG(float n)
{
    return (1.0/sqrt(n + 1.0) - 1.0)/n;
}

void randThetaPhi(vec3 id[4], uvec2 randKey, out vec2 tp[4])
{
    for(int i=0; i<4; ++i)
    {
        vec4 rand = uintToFloat(philox4x32_7(uvec4(ivec3(id[i]), 0), randKey));

        tp[i] = vec2(
            2.0*PI*rand.x,
            acos(2.0*rand.y - 1.0)
        );
    }
}

vec3 smplxNoise3DDeriv(vec3 x, float m, vec3 g)
{
    vec3 dmdxy = max(vec3(0.5) - dot(x, x), 0.0);
    dmdxy = -8.0*x*dmdxy*dmdxy*dmdxy;
    return dmdxy*dot(x, g) + m*g;
}

float smplxNoise3D(vec3 p, out vec3 deriv, uvec2 randKey)
{
    vec3 id[4];
    id[0] = floor(p + vec3( (p.x + p.y + p.z)*skewF(3.0) ));
    float unskew = unskewG(3.0);
    vec3 x[4];
    x[0] = p - (id[0] + vec3( (id[0].x + id[0].y + id[0].z)*unskew ));

    vec3 cmp1 = step(vec3(0.0), x[0] - x[0].zxy);
    vec3 cmp2 = vec3(1.0) - cmp1.yzx; //= step(vec3(0.0), x[0] - x[0].yzx);
    vec3 ii1 = cmp1*cmp2;    //Largest component is 1.0, others are 0.0.
    vec3 ii2 = min(cmp1 + cmp2, 1.0);    //Smallest component is 0.0, others are 1.0.
    vec3 ii3 = vec3(1.0);

    x[1] = x[0] - ii1 - vec3(unskew);
    x[2] = x[0] - ii2 - vec3(2.0*unskew);
    x[3] = x[0] - ii3 - vec3(3.0*unskew);

    float m[4];
    for(int i=0; i<4; ++i)
    {
        m[i] = max(0.5 - dot(x[i], x[i]), 0.0);
        m[i] = m[i]*m[i];
        m[i] = m[i]*m[i];
    }

    id[1] = id[0]+ii1;
    id[2] = id[0]+ii2;
    id[3] = id[0]+ii3;

    vec2 tp[4];
    randThetaPhi(id, randKey, tp);

    //Gradients;
    vec3 g[4];
    for(int i=0; i<4; ++i)
    {
        float r = sin(tp[i].y);
        g[i] = vec3(r*cos(tp[i].x), r*sin(tp[i].x), cos(tp[i].y));
    }

    float ret = 0.0;
    deriv = vec3(0.0);
    for(int i=0; i<4; ++i)
    {
        ret += m[i] * dot(x[i], g[i]);
        deriv += smplxNoise3DDeriv(x[i], m[i], g[i]);
    }

    return ret;
}

float fbm(vec3 p, out vec3 deriv, uvec2 randKey)
{
    float sum = 0.0;
    vec3 deriv_sum = vec3(0.0);
    float s = 1.0;
    for(int i=0; i<2; ++i)
    {
        vec3 d;
    	sum += smplxNoise3D(p*s, d, randKey+uvec2(i))/s;
        deriv_sum += d;
        s *= 1.6;
    }
    deriv = deriv_sum;
    return sum;
}

void mainImage( out vec4 fragColor, in vec2 uv )
{
    vec3 deriv;
//    float c = smplxNoise3D(vec3(uv*2., time), deriv, uvec2(0xfeedbeefu, 0xacebeefu));
//    float c = fbm(vec3(uv*2., time*0.5), deriv, uvec2(0xfeedbeefu, 0xacebeefu));
    vec3 p0, p1, pc;
    for(int i=0; i<3; ++i)
    {
        float c = smplxNoise3D(vec3(uv*2., time*0.5), deriv, uvec2(0xfeedbeefu+uint(i+3), 0xcebeefu));
        float cn = c*54.0 + 0.5;
        float dd = dot(deriv, deriv);
        float e = 1.0/(1.0+c*c*4000./dd);
        float e2 = 1.0/(1.0+c*c*300000000.*dd);

        p0[i] = e;
        p1[i] = e2;
        pc[i] = cn;
    }
    vec3 p = max(p0/p1, pc*1.4-.8);
    float c = smplxNoise3D(p*0.2+1.15, deriv, uvec2(0xbeedbeefu, 0xacebeefu));
    float cn = c*54.0 + 0.5;
    fragColor = vec4(cn);
    float dd = dot(deriv, deriv);
    float e = 1.0/(1.0+c*c*4000./dd);
    float e2 = 1.0/(1.0+c*c*300000000.*dd);
    fragColor = vec4(e);
    return; 
    float ddn = dd * 227.0;
    fragColor = vec4(ddn);
}

void main()
{
  float d = sqrt(dot(uv, uv));
  vec4 fragColor;
  mainImage(fragColor, uv);
  if(d + fragColor.r*0.3 < 1.1)
    discard;
  fragColor *= 0.1;
  fragColor.g += 0.4;
//	color = vec4(abs(uv), time*0.01, 0.0);
  color = fragColor;
}
