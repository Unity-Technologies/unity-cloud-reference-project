fixed4 checker_board(float2 uv, float width, float height, float pixelSize, fixed4 color1, fixed4 color2)
{
    const float2 checker_uv = floor(uv.xy * float2(width, height) / pixelSize);
    const float check = frac((checker_uv.x + checker_uv.y) * 0.5) * 2.0;
    return check > .5 ? color1 : color2;
}

inline float inverseLerp(float from, float to, float value)
{
    return (value - from) / (to - from);
}





// Source: https://iquilezles.org/articles/distfunctions2d/
float roundedBoxSDF(float2 center_position, const float2 size, float4 radius)
{
    radius.xy = (center_position.x > 0.0) ? radius.xy : radius.zw;
    radius.x  = (center_position.y > 0.0) ? radius.x  : radius.y;

    const float2 q = abs(center_position)-size+radius.x;
    return min(max(q.x,q.y),0.0) + length(max(q,0.0)) - radius.x;
}

// Function from Iñigo Quiles
// https://www.shadertoy.com/view/MsS3Wc
float3 hsv_to_rgb(const float3 c)
{
    const float4 k = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    const float3 p = abs(frac(c.xxx + k.xyz) * 6.0 - k.www);
    return c.z * lerp(k.xxx, clamp(p - k.xxx, 0.0, 1.0), c.y);
}




// Source: http://madebyevan.com/shaders/fast-rounded-rectangle-shadows/
// License: CC0 (http://creativecommons.org/publicdomain/zero/1.0/)

// A standard gaussian function, used for weighting samples
float gaussian(float x, float sigma) {
    const float pi = 3.141592653589793;
    return exp(-(x * x) / (2.0 * sigma * sigma)) / (sqrt(2.0 * pi) * sigma);
}

// This approximates the error function, needed for the gaussian integral
float2 erf2(float2 x)
{
    float2 s = sign(x), a = abs(x);
    x = 1.0 + (0.278393 + (0.230389 + 0.078108 * (a * a)) * a) * a;
    x *= x;
    return s - s / (x * x);
}

float4 erf4(float4 x)
{
    float4 s = sign(x), a = abs(x);
    x = 1.0 + (0.278393 + (0.230389 + 0.078108 * (a * a)) * a) * a;
    x *= x;
    return s - s / (x * x);
}

// Return the mask for the shadow of a box from lower to upper
float boxShadow(float2 lower, float2 upper, float2 pos, float sigma)
{
    float4 query = float4(pos - lower, upper - pos);
    float4 integral = 0.5 + 0.5 * erf4(query * (sqrt(0.5) / sigma));
    return min(integral.z, integral.x) * min(integral.w, integral.y);
}

// Return the blurred mask along the x dimension
float roundedBoxShadowX(float x, float y, float sigma, float corner, float2 halfSize)
{
    float delta = min(halfSize.y - corner - abs(y), 0.0);
    float curved = halfSize.x - corner + sqrt(max(0.0, corner * corner - delta * delta));
    float2 integral = 0.5 + 0.5 * erf2((x + float2(-curved, curved)) * (sqrt(0.5) / sigma));
    return integral.y - integral.x;
}

// Return the mask for the shadow of a box from lower to upper
float roundedBoxShadow(float2 lower, float2 upper, float2 pos, float sigma, float corner)
{
    // Center everything to make the math easier
    float2 center = (lower + upper) * 0.5;
    float2 halfSize = (upper - lower) * 0.5;
    pos -= center;

    // The signal is only non-zero in a limited range, so don't waste samples
    float low = pos.y - halfSize.y;
    float high = pos.y + halfSize.y;
    float start = clamp(-3.0 * sigma, low, high);
    float end = clamp(3.0 * sigma, low, high);

    // Accumulate samples (we can get away with surprisingly few samples)
    float step = (end - start) / 4.0;
    float y = start + step * 0.5;
    float value = 0.0;
    for (int i = 0; i < 4; i++) {
        value += roundedBoxShadowX(pos.x, pos.y - y, sigma, corner, halfSize) * gaussian(y, sigma) * step;
        y += step;
    }

    return value;
}
