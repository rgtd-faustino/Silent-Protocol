Shader "SilentProtocol/GlitchHighlight"
{
    Properties
    {
        _Intensity      ("Intensity",       Range(0,1)) = 0
        _Color          ("Color",           Color)      = (0.0, 0.95, 0.75, 1)
        _PulseSpeed     ("Pulse Speed",     Float)      = 2.0
        _EchoOpacity    ("Edge Glow",       Float)      = 0.6
        _WireThickness  ("Wire Thickness",  Float)      = 0.8
    }

    SubShader
    {
        Tags
        {
            "RenderType"      = "Transparent"
            "Queue"           = "Transparent+1"
            "RenderPipeline"  = "UniversalPipeline"
            "DisableBatching" = "True"
        }

        Pass
        {
            Name "GlitchFX"
            Tags { "LightMode" = "SRPDefaultUnlit" }

            Blend SrcAlpha One
            ZWrite Off
            ZTest LEqual
            Cull Back

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #pragma target   3.0
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float  _Intensity;
                float4 _Color;
                float  _PulseSpeed;
                float  _EchoOpacity;
                float  _WireThickness;
            CBUFFER_END

            struct Attributes
            {
                float4 posOS  : POSITION;
                float3 normOS : NORMAL;
                float3 bary   : TEXCOORD1;
            };

            struct Varyings
            {
                float4 posHCS : SV_POSITION;
                float3 normWS : TEXCOORD0;
                float3 viewWS : TEXCOORD1;
                float3 bary   : TEXCOORD2;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                // pequeno offset para evitar z-fighting com o material base
                float3 pos = IN.posOS.xyz + IN.normOS * 0.003;
                OUT.posHCS = TransformObjectToHClip(float4(pos, 1.0));
                OUT.normWS = TransformObjectToWorldNormal(IN.normOS);
                OUT.viewWS = GetWorldSpaceViewDir(TransformObjectToWorld(IN.posOS.xyz));
                OUT.bary   = IN.bary;
                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                float pulse   = 0.5 + 0.5 * sin(_Time.y * _PulseSpeed);
                float pulse2  = 0.7 + 0.3 * sin(_Time.y * _PulseSpeed * 0.7 + 1.3);

                // ── WIREFRAME ──────────────────────────────────────────────
                float3 bary    = IN.bary;
                float  minB    = min(bary.x, min(bary.y, bary.z));
                float  wire    = 1.0 - smoothstep(0.0, fwidth(minB) * _WireThickness, minB);
                float  flicker = 0.85 + 0.15 * sin(_Time.y * 37.3 + bary.x * 10.0);
                float  wireA   = wire * pulse * flicker;

                // ── FRESNEL (brilho nas arestas / silhueta) ────────────────
                float fresnel  = 1.0 - saturate(dot(normalize(IN.normWS), normalize(IN.viewWS)));
                fresnel        = pow(fresnel, 1.8);
                float fresnelA = fresnel * _EchoOpacity * pulse2;

                // combina os dois — a wireframe domina, o fresnel enche os contornos
                float alpha = saturate(wireA + fresnelA) * _Intensity;
                return float4(_Color.rgb, alpha);
            }
            ENDHLSL
        }
    }
}
