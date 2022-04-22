Shader "DVRSDK/Mirror"
{
	Properties
	{
		_MainTex("Base (RGB)", 2D) = "white" {}
		[HideInInspector] _ReflectionTex("", 2D) = "white" {}
		//0:NonStereo 1:従来方法 2:VirtualDesktop対応
		[HideInInspector] _StereoMode("Stereo Mode", int) = 0
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
			int _StereoMode;

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 refl : TEXCOORD1;
				float4 pos : SV_POSITION;

				UNITY_VERTEX_OUTPUT_STEREO
			};

			v2f vert(VertexInput v)
			{
				v2f o;

				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv0, _MainTex);
				o.refl = ComputeScreenPos(o.pos);

				// シングルパスステレオレンダリングではない時ComputeScreenPosはサイドバイサイド画像から正しい座標を返しません
				// ProjectionMatrixの水平方向のスキューが0より小であれば左目用で、0より大であれば右目用のレンダリングパスです

				// OculusのVirtual DesktopだとProjectionMatrixで右目左目を取れないのでunity_StereoEyeIndexで取得するように変更
				// デスクトップでも左目の処理に入ってしまうため、StermVRMirror.csからStereoの情報を入力

#ifndef UNITY_SINGLE_PASS_STEREO				
				if (_StereoMode == 1)
				{
					if (unity_CameraProjection[0][2] < 0)
					{
						o.refl.x = (o.refl.x * 0.5f);
					}
					else if (unity_CameraProjection[0][2] > 0)
					{
						o.refl.x = (o.refl.x * 0.5f) + (o.refl.w * 0.5f);
					}
				}
				else if (_StereoMode == 2)
				{
					if (unity_StereoEyeIndex == 0)
					{
						o.refl.x = (o.refl.x * 0.5f);
					}
					else if (unity_StereoEyeIndex == 1)
					{
						o.refl.x = (o.refl.x * 0.5f) + (o.refl.w * 0.5f);
					}
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
