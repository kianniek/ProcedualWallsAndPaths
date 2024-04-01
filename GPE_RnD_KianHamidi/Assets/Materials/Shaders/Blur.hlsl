// Example HLSL code for a simple blur effect
void GaussianBlur(float2 texSize, float2 uv : TEXCOORD0, out float4 Out : SV_Target, Texture2D<float4> _MainTex, SamplerState _MainTexSampler, float _BlurRadius)
{
    float2 texelSize = 1.0 / texSize;
    float3 result = float3(0, 0, 0);
    
    // Simple blur algorithm; adjust the offsets and weights for different blur effects
    for (int x = -2; x <= 2; x++)
    {
        for (int y = -2; y <= 2; y++)
        {
            float2 sampleUV = uv + float2(x, y) * texelSize * _BlurRadius;
            result += _MainTex.Sample(_MainTexSampler, sampleUV).rgb;
        }
    }

    result /= 25.0; // Normalize by the number of samples
    Out = float4(result, 1.0);
}
