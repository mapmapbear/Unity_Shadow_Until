Shader "Shader/Caster"
{
	SubShader{
		Tags {
			"RenderType" = "Opaque"
		}

		Pass {
			Fog { Mode Off }
			Cull Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct v2f {
				float4 pos : SV_POSITION;
				float2 depth : TEXCOORD0;
			};


			v2f vert(appdata_full v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				//o.pos.z += _gShadowBias;
				o.depth = o.pos.zw;

				return o;
			}

			fixed4 frag(v2f i) : COLOR
			{
				return 1;
			}
			ENDCG
		}
	}
}