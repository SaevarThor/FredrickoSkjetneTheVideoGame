Shader "Custom/PS2PostProcess"
{
    Properties
    {
        _MainTex        ("Texture",             2D)     = "white" {}
        _ColorBands     ("Color Bands",         Float)  = 16
        _DitherStrength ("Dither Strength",     Float)  = 0.4
        _ScanlineStrength("Scanline Strength",  Float)  = 0.15
        _ScanlineThickness("Scanline Thickness",Float)  = 1
        _Resolution     ("Internal Resolution", Vector) = (480, 272, 0, 0)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        ZWrite Off ZTest Always Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float _ColorBands;
            float _DitherStrength;
            float _ScanlineStrength;
            float _ScanlineThickness;
            float4 _Resolution;

            // ----------------------------------------------------------------
            // Bayer 4x4 dithering matrix
            // Gives that classic retro ordered dither, not random noise.
            // ----------------------------------------------------------------
            float BayerDither4x4(float2 pos)
            {
                int2 p = int2(fmod(pos, 4));
                float4x4 bayer = float4x4(
                     0,  8,  2, 10,
                    12,  4, 14,  6,
                     3, 11,  1,  9,
                    15,  7, 13,  5
                );
                // Read from matrix (HLSL doesn't support dynamic indexing of float4x4 cleanly,
                // so we flatten it)
                float table[16] = {0,8,2,10, 12,4,14,6, 3,11,1,9, 15,7,13,5};
                int idx = p.y * 4 + p.x;
                return table[idx] / 16.0;
            }

            // ----------------------------------------------------------------
            // Color banding — quantize each channel to N steps
            // ----------------------------------------------------------------
            float3 BandColor(float3 col, float bands)
            {
                return floor(col * bands) / bands;
            }

            // ----------------------------------------------------------------
            // Fragment shader
            // ----------------------------------------------------------------
            fixed4 frag(v2f_img i) : SV_Target
            {
                // Snap UV to internal resolution grid (pixelate)
                float2 pixelUV = floor(i.uv * _Resolution.xy) / _Resolution.xy;
                float3 col = tex2D(_MainTex, pixelUV).rgb;

                // Dithering — offset color before banding so edges get nice patterns
                float2 screenPos = pixelUV * _Resolution.xy;
                float dither = BayerDither4x4(screenPos) - 0.5; // -0.5..0.5
                col += dither * _DitherStrength * (1.0 / _ColorBands);

                // Color banding
                col = BandColor(saturate(col), _ColorBands);

                // Scanlines — darken every Nth row
                float scanlineRow = fmod(floor(pixelUV.y * _Resolution.y), _ScanlineThickness * 2.0);
                float scanline = (scanlineRow < _ScanlineThickness) ? 1.0 : (1.0 - _ScanlineStrength);
                col *= scanline;

                // Slight warm tint — PS2 CRTs had a warm yellow-green cast
                col *= float3(1.02, 1.0, 0.95);

                return fixed4(col, 1.0);
            }
            ENDCG
        }
    }
}
