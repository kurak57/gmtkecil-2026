Shader "Custom/CRT_UI"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        [Header(CRT Distortion)]
        _Distortion ("Distortion (Kecembungan)", Range(0, 1)) = 0.15
        _Zoom ("Zoom (Perbesar untuk sembunyikan tepi)", Range(0.5, 2)) = 1.0
        
        [Header(Scanlines)]
        _ScanlineAmount ("Scanline Amount (Banyak Garis)", Range(0, 2000)) = 800
        _ScanlineThickness ("Scanline Thickness (Ketebalan)", Range(0, 1)) = 0.5
        _ScanlineColor ("Scanline Color", Color) = (0,0,0,0.5)
        
        /* Stencil properties required for UI Masking */
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        { 
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            float _Distortion;
            float _Zoom;
            float _ScanlineAmount;
            float _ScanlineThickness;
            fixed4 _ScanlineColor;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.worldPosition = IN.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // Barrel distortion (Cembung)
                float2 uv = IN.texcoord;
                
                // Shift UV to -1 to 1 range (center origin)
                uv = uv * 2.0 - 1.0;
                
                // Zoom UV (to hide edges if desired)
                uv /= _Zoom;
                
                // Calculate barrel distortion
                float rsq = uv.x*uv.x + uv.y*uv.y;
                uv += uv * (rsq * _Distortion);
                
                // Shift back to 0 to 1 range
                uv = uv * 0.5 + 0.5;

                // Check out of bounds (transparent instead of black border)
                if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0)
                {
                    return fixed4(0,0,0,0); // Transparent
                }

                // Sample texture
                half4 color = tex2D(_MainTex, uv) * IN.color;
                
                // Scanlines (Noise garis-garis)
                float scanline = sin(uv.y * _ScanlineAmount * 3.14159);
                
                // Remap sin wave and apply thickness threshold
                float scanlineMask = smoothstep(_ScanlineThickness - 0.1, _ScanlineThickness + 0.1, scanline);
                
                // Blend color with scanline color (based on alpha)
                color.rgb = lerp(color.rgb, _ScanlineColor.rgb, (1.0 - scanlineMask) * _ScanlineColor.a);

                return color;
            }
            ENDCG
        }
    }
}
