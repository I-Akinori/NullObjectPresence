Shader "Custom/VirtualStandard"
{
    Properties
    {
        _AnalogColor ("AnalogColor", Color) = (1,1,1,1)
        _DigitalColor ("DigitalColor", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        
        _EffectColor ("EffectColor", Color) = (1, 1, 1, 1)
        _Center ("Center", Vector) = (0, 0, 0, 0)
        _Freq ("Freq", float) = 0.5
        _Speed ("Speed", float) = 0.5
        _Value ("Value", Range(0, 1)) = 0.5
        
        _ADRatio ("ADRatio", Range(0, 1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
        
        CGPROGRAM

        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0
        
        #include "UnityCG.cginc"

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _AnalogColor;
        fixed4 _DigitalColor;
        float _ADRatio;

        float4 _EffectColor;
        float4 _Center;
        float _Freq;
        float _Speed;
        float _Value;

        float4 _DistortionCenter[16];
        float _DistortionIntensity[16];

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        float3 distortion(float2 p)
        {
            float bright = 0;
            float phi0 = _Time.y * 0.1;
            for (uint i = 0; i < 16; i++)
            {
                float phi = phi0 + i * 3.141592 / 8;
                p -= _DistortionCenter[i];
                float theta = atan2(p.y, p.x);
                float rad = length(p);
                theta += _DistortionIntensity[i] * 3 * exp(-rad * 10) * (i % 2 == 0 ? 1 : -1);
                p.x = rad * cos(theta);
                p.y = rad * sin(theta);
                p += _DistortionCenter[i];
                bright += saturate(
                    exp(-rad * 50) * _DistortionIntensity[i] * sin(theta * 4 + 5 * _Time.y * (i % 2 == 0 ? 1 : -1)));
            }
            return float3(p, bright);
        }
        float4 grid(float2 uv)
        {
            /*
            for (uint i = 0; i < 16; i++)
            {
                if (length(uv - _DistortionCenter[i]) < 0.1)
                {
                    return float4(1, 0, 0, 1);
                }
            }*/
            //return float4(_DistortionIntensity[0], _DistortionIntensity[1], _DistortionIntensity[2], 1);
            float2 localUV = (uv * 16) % 1;
            localUV.x += localUV.x < 0 ? 1 : 0;
            localUV.y += localUV.y < 0 ? 1 : 0; 
            float lineWidth = 0.01;
            if (localUV.x < lineWidth ||
                localUV.x > 1 - lineWidth ||
                localUV.y < lineWidth ||
                localUV.y > 1 - lineWidth)
            {
                return float4(1, 1, 1, 1) * _ADRatio * 0.2;
            }
            return float4(0, 0, 0, 1);
        }
        float4 sphere_wave(float3 pos, float3 cen, float freq, float speed, float val, float4 col)
            {
                float len = length(pos - cen) / (0.001 + val);
                float v01 = 0;
                if (len * freq - val * speed < 3)
                {
                    v01 = fmod(len * freq - val * speed, 1.0);
                    v01 = v01 < 0 ? v01 + 1 : v01;
                    v01 = exp(-v01 * 2.0) * (1 - val);
                    v01 = saturate(v01);
                }
                float4 clear = float4(0, 0, 0, 0);
                return lerp(clear, col, v01);
            }
        
        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * lerp(_AnalogColor, _DigitalColor, _ADRatio);
            float2 uv = IN.uv_MainTex;
            float3 uvb = distortion(uv);
            c += grid(uvb.xy);
            c += uvb.z;
            c += sphere_wave(IN.worldPos, _Center.xyz, _Freq, _Speed, _Value, _EffectColor);
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
