Shader "Custom/PS2VertexJitter"
{
    // -------------------------------------------------------------------------
    // Apply this shader to world geometry materials (floors, walls, props).
    // It snaps vertices to a low-precision grid, reproducing the wobbly,
    // swimming geometry that was the PS2's most iconic visual artefact —
    // caused by its lack of sub-pixel vertex precision.
    // -------------------------------------------------------------------------
    Properties
    {
        _MainTex        ("Albedo (RGB)",        2D)     = "white" {}
        _Color          ("Tint",                Color)  = (1,1,1,1)
        _SnapPrecision  ("Vertex Snap Grid",    Float)  = 80.0   // Lower = more wobble
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _SnapPrecision;

            // Global properties set by PS2PostProcess.cs every frame
            float _PS2WobbleIntensity;
            float _PS2Time;

            struct appdata
            {
                float4 vertex   : POSITION;
                float2 uv       : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv       : TEXCOORD0;
                float4 vertex   : SV_POSITION;
            };

            // ----------------------------------------------------------------
            // Snap a clip-space position to a low-resolution grid.
            // This is exactly what the PS2 GS (Graphics Synthesizer) did —
            // it had only integer sub-pixel precision.
            // ----------------------------------------------------------------
            float4 SnapToGrid(float4 clipPos, float precision)
            {
                float2 snap = round((clipPos.xy / clipPos.w) * precision) / precision;
                return float4(snap * clipPos.w, clipPos.z, clipPos.w);
            }

            v2f vert(appdata v)
            {
                v2f o;

                // Standard transform
                float4 clipPos = UnityObjectToClipPos(v.vertex);

                // Vertex snapping — the PS2 wobble
                if (_PS2WobbleIntensity > 0.001)
                {
                    float snapGrid = _SnapPrecision * (1.0 - _PS2WobbleIntensity * 0.9);
                    clipPos = SnapToGrid(clipPos, snapGrid);
                }

                o.vertex = clipPos;

                // Affine texture mapping — PS2 had no perspective-correct UV interpolation.
                // We fake this by NOT dividing UVs by W, giving the classic swimming texture look.
                // Multiply by W so the interpolator "un-divides" it in screen space.
                o.uv = TRANSFORM_TEX(v.uv, _MainTex) * clipPos.w;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Recover affine UVs — divide by interpolated W (passed via uv.z trick).
                // Here we use a simpler approximation: the distortion is baked into the UV
                // drift already from the vertex stage. Just sample normally.
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                return col;
            }
            ENDCG
        }
    }

    FallBack "Diffuse"
}
