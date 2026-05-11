Shader "SilentProtocol/GlitchHighlight"
{
    Properties
    {
        _Intensity      ("Intensity",       Range(0,1)) = 0
        _Color          ("Color",           Color)      = (0.0, 0.95, 0.75, 1)
        _PulseSpeed     ("Pulse Speed",     Float)      = 2.0
        _EchoDistance   ("Echo Distance",   Float)      = 0.04
        _EchoOpacity    ("Echo Opacity",    Float)      = 0.35
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

        // ─────────────────────────────────────────────────────────────────────
        // PASS 1 — ECHO HOLOGRÁFICO
        // ─────────────────────────────────────────────────────────────────────
        Pass
        {
            Name "HoloEcho"
            Tags { "LightMode" = "SRPDefaultUnlit" }

            Blend SrcAlpha One
            ZWrite Off
            ZTest LEqual
            Cull Front

            HLSLPROGRAM
            #pragma vertex   vert_echo
            #pragma fragment frag_echo
            #pragma target 3.0
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float  _Intensity;
                float4 _Color;
                float  _PulseSpeed;
                float  _EchoDistance;
                float  _EchoOpacity;
                float  _WireThickness;
            CBUFFER_END

            struct Attributes { float4 posOS : POSITION; float3 normOS : NORMAL; };
            struct Varyings   { float4 posHCS : SV_POSITION; float3 normWS : TEXCOORD0; float3 viewWS : TEXCOORD1; };

            Varyings vert_echo(Attributes IN)
            {
                Varyings OUT;
                float3 expanded = IN.posOS.xyz + IN.normOS * _EchoDistance * _Intensity;
                OUT.posHCS = TransformObjectToHClip(float4(expanded, 1.0));
                OUT.normWS = TransformObjectToWorldNormal(IN.normOS);
                OUT.viewWS = GetWorldSpaceViewDir(TransformObjectToWorld(IN.posOS.xyz));
                return OUT;
            }

            float4 frag_echo(Varyings IN) : SV_Target
            {
                float fresnel = 1.0 - saturate(dot(normalize(IN.normWS), normalize(IN.viewWS)));
                fresnel = pow(fresnel, 1.4);
                float pulse = 0.7 + 0.3 * sin(_Time.y * _PulseSpeed);
                float alpha = fresnel * _EchoOpacity * _Intensity * pulse;
                return float4(_Color.rgb, alpha);
            }
            ENDHLSL
        }

        // ─────────────────────────────────────────────────────────────────────
        // PASS 2 — WIREFRAME
        // ─────────────────────────────────────────────────────────────────────
        Pass
        {
            Name "Wireframe"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha One
            ZWrite Off
            ZTest LEqual
            Cull Back

            HLSLPROGRAM
            #pragma vertex   vert_wire
            #pragma fragment frag_wire
            #pragma target 3.0
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float  _Intensity;
                float4 _Color;
                float  _PulseSpeed;
                float  _EchoDistance;
                float  _EchoOpacity;
                float  _WireThickness;
            CBUFFER_END

            struct Attributes
            {
                float4 posOS  : POSITION;
                float3 normOS : NORMAL;
                float2 uv1    : TEXCOORD0;
                float3 bary   : TEXCOORD1;
            };

            struct Varyings
            {
                float4 posHCS : SV_POSITION;
                float3 bary   : TEXCOORD0;
            };

            Varyings vert_wire(Attributes IN)
            {
                Varyings OUT;
                float3 pos = IN.posOS.xyz + IN.normOS * 0.002 * _Intensity;
                OUT.posHCS = TransformObjectToHClip(float4(pos, 1.0));
                OUT.bary   = IN.bary;
                return OUT;
            }

            float4 frag_wire(Varyings IN) : SV_Target
            {
                float3 bary    = IN.bary;
                float  minB    = min(bary.x, min(bary.y, bary.z));
                float  wire    = 1.0 - smoothstep(0.0, fwidth(minB) * _WireThickness, minB);
                float  pulse   = 0.5 + 0.5 * sin(_Time.y * _PulseSpeed * 2.5);
                float  flicker = 0.85 + 0.15 * sin(_Time.y * 37.3 + bary.x * 10.0);
                float  alpha   = wire * _Intensity * pulse * flicker;
                return float4(_Color.rgb, alpha);
            }
            ENDHLSL
        }
    }
}
