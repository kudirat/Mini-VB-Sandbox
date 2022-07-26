Shader "X-Ray"
{
	Properties
	{
		//blend between main and secondary colors
		_MainColor("Main Color", Color) = (0, 0, 0, 1)
		_SecondaryColor("Secondary Color", Color) = (1, 1, 1, 1)
		_Blend("Blend", Range(0, 4)) = 1

		//fresnel effect
		_Fade("Fade", Range(0, 4)) = 2
		_MinimumOpacity("Minimum Opacity", Range(0, 1)) = 0
		[Enum(Fresnel, 0, InvertedFresnel, 1)] _InvertFade("Invert Fade", Float) = 0

		//fadeout when the camera gets too close
		_FadeoutDistance("Fadeout Distance", Float) = 0
		_FadeoutRange("Fadeout Range", Float) = 0

		//pulse
		[Enum(NoPulse, 0, Pulse, 1)] _AnimatePulse("Animate Pulse", Float) = 0
		_PulseStrength("Pulse Strength", Range(0, 1)) = .1
		_PulseSpeed("Pulse Speed", Range(.1, 10)) = 1

		//shader property enums
		[Enum(Off, 0, On, 1)] _ZWrite("ZWrite", Float) = 0
		[Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull", Float) = 2 //Back
		[Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("ZTest", Float) = 4 //LessEqual
		[Enum(UnityEngine.Rendering.BlendMode)] _BlendSrc("Blend Src", Float) = 1 //One
		[Enum(UnityEngine.Rendering.BlendMode)] _BlendDst("Blend Dst", Float) = 10 //OneMinusSrcAlpha
	}

	SubShader
	{
		Tags
		{
			"Queue"="Transparent" 
			"RenderType"="Transparent"
		}

		//multipass shaders unsupported in URP
		//Pass
		//{	
		//	//first pass: write depth buffer
		//	ZWrite[_ZWrite]
		//	ColorMask 0
		//}

		Pass
		{	
			//second pass: draw the object
			ZWrite Off
			ZTest[_ZTest]
			Cull[_Cull]
			Blend[_BlendSrc][_BlendDst]
			Lighting Off
			ColorMask RGBA

			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vert
			#pragma fragment frag
			
			struct Input
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float4 normal	: NORMAL;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
			};
			
			fixed4 _MainColor;
			fixed4 _SecondaryColor;
			float _Blend;

			float _Fade;
			float _MinimumOpacity;
			float _InvertFade;

			float _FadeoutDistance;
			float _FadeoutRange;

			float _AnimatePulse;
			float _PulseStrength;
			float _PulseSpeed;

			v2f vert(Input IN)
			{
				v2f OUT;

				//set vertex
				OUT.vertex = UnityObjectToClipPos(IN.vertex);

				//calculate fresnel effect
				float dotViewDirNormal = dot(normalize(ObjSpaceViewDir(IN.vertex)), IN.normal);
				float fresnel = 1 - abs(dotViewDirNormal);
				fresnel = ((1 - _InvertFade) * fresnel) + (_InvertFade * (1 - fresnel));

				//blend between main and secondary colors
				float blendAmount = pow(fresnel, _Blend);
				fixed4 blendedColor = lerp(_MainColor, _SecondaryColor, blendAmount);

				//set color
				OUT.color.rgb = IN.color.rgb * blendedColor.rgb;
				float colorAlpha = IN.color.a * blendedColor.a;

				//calculate fresnel fading
				float edgeFadeAmount = pow(fresnel, _Fade);
				float fadeAlpha = saturate(edgeFadeAmount);

				//calculate pulse
				float pulseAmount = _PulseStrength * ((sin(_Time.w * _PulseSpeed) + 1) / 2);
				float pulseAlpha = _AnimatePulse * pulseAmount;

				//fade when near camera
				float distanceFromCFMin = length(_WorldSpaceCameraPos - mul(unity_ObjectToWorld, IN.vertex).xyz) - _FadeoutDistance;
				float cameraFadeAlpha = saturate(distanceFromCFMin / _FadeoutRange);

				//set alpha
				OUT.color.a = max((fadeAlpha * colorAlpha) - pulseAlpha, _MinimumOpacity) * cameraFadeAlpha;

				return OUT;
			}

			fixed4 frag(v2f IN) : SV_Target
			{
				fixed4 c = IN.color;
				c.rgb *= c.a;
				return c;
			}
		ENDCG
		}
	}
}
