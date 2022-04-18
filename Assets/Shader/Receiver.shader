Shader "Shader/Receiver" {
	SubShader{
		Tags { "RenderType" = "Opaque" }
		LOD 300

		Pass {
			Name "FORWARD"
			Tags{ "LightMode" = "ForwardBase" }

			CGPROGRAM
			#include "UnityCG.cginc"
			#include "UnityLightingCommon.cginc"

			struct v2f
			{
				float4 pos : SV_POSITION;
				float4 shadowCoord : TEXCOORD0;
				half3 normal : NORMAL;
				float4 shadowCoord1 : TEXCOORD5;
				float4 worldPos : TEXCOORD3;
			};
			uniform float4x4 _gWorldToShadow;
			uniform float4x4 _gSpotWorldToShadow;
			uniform float _gShadowStrength;
			UNITY_DECLARE_SHADOWMAP(_gShadowMapTexture);
			UNITY_DECLARE_SHADOWMAP(_gSpotShadowMapTexture);

			v2f vert(appdata_full v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex); // word to view
				float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
				o.shadowCoord = mul(_gSpotWorldToShadow, worldPos);
				o.shadowCoord1 = mul(_gWorldToShadow, worldPos);
				o.normal = UnityObjectToWorldNormal(v.normal);
				o.worldPos = worldPos;
				return o;
			}

			float GetShadowBase(float3 shadowCoord)
			{
				shadowCoord.z += 0.001;
				return UNITY_SAMPLE_SHADOW(_gShadowMapTexture, shadowCoord.xyz);
			}

			

			float SamplePCF(float2 uv, float depth)
			{
				float shadow = 0;
				int _PCFBlur = 2;
				/*#if !defined(UNITY_REVERSED_Z)
						float bias = -0.001;
				#else
						float bias = 0.001;
				#endif*/
				
				#ifdef NO_PCF
						return GetShadowBase(float3(uv, depth));
				#endif

				#ifdef PCF_2X2
						_PCFBlur = 1;
				#endif

				#ifdef PCF_4X4
						_PCFBlur = 2;
				#endif

				#ifdef PCF_8X8
						_PCFBlur = 4;
				#endif

				#ifdef PCF_16X16
						_PCFBlur = 8;
				#endif
				
				float pcf_num = _PCFBlur;
				float2 texSize = float2(2048, 2048);
				for (int i = -pcf_num; i <= pcf_num; ++i)
				{
					for (int j = -pcf_num; j <= pcf_num; ++j)
					{
						float2 uv_offset = float2(i, j) / texSize;
						uv += uv_offset;
						shadow += GetShadowBase(float3(uv, depth));
					}
				}
				shadow /= ((pcf_num + pcf_num + 1) * (pcf_num + pcf_num + 1));
				return shadow;
			}

			float4 _SpotLightPos;									//聚光灯位置
			float4 _SpotLightRot;									//聚光灯方向
			float _SpotRange = 1000;											//聚光灯光照范围
			float _SpotAngle = 60;										//聚光灯光照角度
			fixed3 _SpotColor = fixed3(1, 0, 0);										//聚光灯光照颜色
			float _SpotIntensity = 1;										//聚光灯光照强度
			float _Atten = 1;

			inline fixed3 GetSpotLight(half3 worldPos, half3 worldNormal) {
				float3 displacement = _SpotLightPos.xyz - worldPos;
				float distance = length(displacement);
				float3 spotLightDir = displacement / distance;
				fixed3 spotDiffuse = _SpotColor.xyz * max(0, dot(worldNormal, spotLightDir));
				fixed3 spotFinalLight;

				float attenDistance = pow(1 - clamp(distance / _SpotRange, 0, 1), _Atten);
				float attenAngle = (dot(spotLightDir, _SpotLightRot) - cos(radians(_SpotAngle / 2))) /
					(cos(radians(_SpotAngle * 0.75) / 2) - cos(radians(_SpotAngle / 2)));
				float allAtten = attenDistance * attenAngle;
				spotFinalLight = max(0, spotDiffuse * _SpotIntensity * allAtten);
				return spotFinalLight;
			}
			fixed4 frag(v2f i) : SV_Target
			{
				// shadow
				i.shadowCoord.xyz = i.shadowCoord.xyz / i.shadowCoord.w;
				float2 uv = i.shadowCoord.xy;
				float depth = i.shadowCoord.z;
				uv = uv * 0.5 + 0.5;
#if !defined(UNITY_REVERSED_Z) 
				i.shadowCoord.xyz = i.shadowCoord.xyz * 0.5 + 0.5;
				i.shadowCoord1.xyz = i.shadowCoord1.xyz * 0.5 + 0.5;
#else 
				i.shadowCoord.xy = i.shadowCoord.xy * 0.5 + 0.5;
				i.shadowCoord1.xy = i.shadowCoord1.xy * 0.5 + 0.5;
#endif
				float spotShadow = UNITY_SAMPLE_SHADOW(_gSpotShadowMapTexture, i.shadowCoord.xyz);
				float shadow = UNITY_SAMPLE_SHADOW(_gShadowMapTexture, i.shadowCoord1.xyz);
				if (i.shadowCoord.x < 0 || i.shadowCoord.x > 1 || i.shadowCoord.y < 0 || i.shadowCoord.y > 1) shadow = 1;
				if (i.shadowCoord1.x < 0 || i.shadowCoord1.x > 1 || i.shadowCoord1.y < 0 || i.shadowCoord1.y > 1) shadow = 1;
				
				half3 SpotLight = GetSpotLight(i.worldPos, i.normal);
				half3 SpotShadowColor = half3(1, 0, 0);
				//return fixed4(SpotLight, 1);
				half3 normal = normalize(i.normal);
				half3 diffuse = max(dot(normal, _WorldSpaceLightPos0.xyz), 0) * _LightColor0.xyz;
				fixed4 col = 1;
				col.xyz = col.xyz * ((diffuse + SpotLight));
				if(shadow == 0)
				col = fixed4(SpotLight, 1);

				if(spotShadow == 0) col = fixed4(diffuse, 1);
				if(shadow == 0 && spotShadow == 0)	col = 0;
				return col;
			}

			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest  
			ENDCG
		}
	}
}