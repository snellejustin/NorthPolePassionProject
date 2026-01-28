Shader "Custom/URP_Occlusion"
{
    Properties
    {
    }
    SubShader
    {
        // Render EARLY (in Background/Geometry-2000) to ensure we fill Depth before any transparent effects run
        Tags { "RenderType"="Opaque" "Queue"="Geometry-2000" "RenderPipeline" = "UniversalPipeline" }
        LOD 100

        Pass
        {
            Name "Occlusion"
            Tags { "LightMode" = "UniversalForward" }
            
            ColorMask 0 // Do not draw any color (Invisible)
            ZWrite On   // Write to Depth Buffer (Solid obstacle)
            
            // Offset removed: We are handling visual offset in lanceBeam.cs via script now (2cm bias)
            // This ensures the depth buffer is 'accurate' to the wall position.
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                return half4(0,0,0,0);
            }
            ENDHLSL
        }
    }
}
