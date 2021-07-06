Shader "DVRSDK/Mirror"
{
	Properties
	{
		_MainTex("Base (RGB)", 2D) = "white" {}
		[HideInInspector] _ReflectionTex("", 2D) = "white" {}
	}
	SubShader
	{
		Tags{ "RenderType" = "Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityStandardCore.cginc"

			sampler2D _ReflectionTex;

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 refl : TEXCOORD1;
				float4 pos : SV_POSITION;
			};

			v2f vert(VertexInput v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv0, _MainTex);
				o.refl = ComputeScreenPos(o.pos);

                // シングルパスステレオレンダリングではない時ComputeScreenPosはサイドバイサイド画像から正しい座標を返しません
                // ProjectionMatrixの水平方向のスキューが0より小であれば左目用で、0より大であれば右目用のレンダリングパスです
#ifndef UNITY_SINGLE_PASS_STEREO
				if (unity_CameraProjection[0][2] < 0)
				{
					o.refl.x = (o.refl.x * 0.5f);
				}
				else if (unity_CameraProjection[0][2] > 0)
				{
					o.refl.x = (o.refl.x * 0.5f) + (o.refl.w * 0.5f);
				}
#endif

				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 tex = tex2D(_MainTex, i.uv);
				fixed4 refl = tex2Dproj(_ReflectionTex, UNITY_PROJ_COORD(i.refl));
				return tex * refl;
			}
			ENDCG
		}
	}
}
