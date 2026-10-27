Shader "BG/Perlin Dither Background"
{
    Properties
    {
        [HDR] _ColorA ("Dark Color", Color) = (0.035, 0.09, 0.12, 1)
        [HDR] _ColorB ("Light Color", Color) = (0.10, 0.35, 0.30, 1)
        _Threshold ("Noise Threshold", Range(0, 1)) = 0.5
        _DitherStrength ("Dither Strength", Range(0, 0.5)) = 0.16
        _NoisePixelSize ("Noise Size (Pixels)", Range(2, 120)) = 24
        _DriftPixelsPerSecond ("Drift (Pixels / Second)", Vector) = (1.5, 0.75, 0, 0)
        _AnimationFPS ("Animation FPS", Range(1, 1000)) = 12
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Background"
            "RenderType" = "Opaque"
            "PreviewType" = "Plane"
        }

        Cull Off
        ZWrite Off
        ZTest LEqual
        Blend Off

        Pass
        {
            CGPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma target 3.0

            #include "UnityCG.cginc"

            fixed4 _ColorA;
            fixed4 _ColorB;
            float _Threshold;
            float _DitherStrength;
            float _NoisePixelSize;
            float4 _DriftPixelsPerSecond;
            float _AnimationFPS;

            static const float2 kReferenceResolution = float2(240.0, 480.0);

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = UnityObjectToClipPos(input.positionOS);
                output.uv = input.uv;
                return output;
            }

            // Returns a stable, pseudo-random unit vector for each integer lattice point.
            float2 Gradient(float2 lattice)
            {
                float2 value = float2(
                    dot(lattice, float2(127.1, 311.7)),
                    dot(lattice, float2(269.5, 183.3)));

                value = frac(sin(value) * 43758.5453) * 2.0 - 1.0;
                return value * rsqrt(max(dot(value, value), 0.0001));
            }

            // 2D gradient Perlin noise, normalized approximately to the 0..1 range.
            float PerlinNoise(float2 position)
            {
                float2 cell = floor(position);
                float2 local = frac(position);
                float2 fade = local * local * local * (local * (local * 6.0 - 15.0) + 10.0);

                float bottomLeft = dot(Gradient(cell + float2(0, 0)), local - float2(0, 0));
                float bottomRight = dot(Gradient(cell + float2(1, 0)), local - float2(1, 0));
                float topLeft = dot(Gradient(cell + float2(0, 1)), local - float2(0, 1));
                float topRight = dot(Gradient(cell + float2(1, 1)), local - float2(1, 1));

                float bottom = lerp(bottomLeft, bottomRight, fade.x);
                float top = lerp(topLeft, topRight, fade.x);
                return saturate(lerp(bottom, top, fade.y) * 0.70710678 + 0.5);
            }

            // Normalized 4x4 Bayer matrix. Its fixed pattern produces a pixel-art dissolve edge.
            float Bayer4x4(float2 pixel)
            {
                int x = (int)fmod(pixel.x, 4.0);
                int y = (int)fmod(pixel.y, 4.0);
                float value;

                if (y == 0)
                    value = (x == 0) ? 0.0 : (x == 1) ? 8.0 : (x == 2) ? 2.0 : 10.0;
                else if (y == 1)
                    value = (x == 0) ? 12.0 : (x == 1) ? 4.0 : (x == 2) ? 14.0 : 6.0;
                else if (y == 2)
                    value = (x == 0) ? 3.0 : (x == 1) ? 11.0 : (x == 2) ? 1.0 : 9.0;
                else
                    value = (x == 0) ? 15.0 : (x == 1) ? 7.0 : (x == 2) ? 13.0 : 5.0;

                return (value + 0.5) / 16.0;
            }

            fixed4 Frag(Varyings input) : SV_Target
            {
                // One shader cell always corresponds to one pixel in the 240x480 reference image.
                float2 pixel = min(floor(saturate(input.uv) * kReferenceResolution),
                                   kReferenceResolution - 1.0);

                // Quantizing time prevents smooth texture swimming and gives deliberate pixel motion.
                float fps = max(_AnimationFPS, 1.0);
                float steppedTime = floor(_Time.y * fps) / fps;
                float noisePixelSize = max(_NoisePixelSize, 1.0);
                float2 noisePosition =
                    (pixel + steppedTime * _DriftPixelsPerSecond.xy) / noisePixelSize;

                float noise = PerlinNoise(noisePosition);
                float ditherOffset = (Bayer4x4(pixel) - 0.5) * _DitherStrength;
                float binaryNoise = step(_Threshold, noise + ditherOffset);

                return lerp(_ColorA, _ColorB, binaryNoise);
            }
            ENDCG
        }
    }

    Fallback Off
}