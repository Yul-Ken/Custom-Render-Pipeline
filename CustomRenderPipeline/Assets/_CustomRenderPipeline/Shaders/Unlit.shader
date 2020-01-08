Shader "CRP/Unlit"
{
   Properties{
    _BaseColor("Color", Color) = (1.0, 1.0, 1.0, 1.0)
   [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Src Blend",float)=1
   [Enum(UnityEngine.Rendering.BlendMode)] _DesBlend("Des Blend",float)=0

   }

   SubShader{
   
   Pass{
            Blend [_SrcBlend] [_DesBlend]
            HLSLPROGRAM
                #pragma multi_compile_instancing
                #pragma vertex UnlitPassVertex
                #pragma fragment UnlitPassFragment
                #include "UnlitPass.hlsl"
            ENDHLSL
        }
   
   }
}
