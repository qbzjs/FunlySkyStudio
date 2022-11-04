// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "CircleFog"
{
	Properties
	{
		_Color("Color ", Color) = (0,0,0,0)
		_Color0("Color 0", Color) = (0,0,0,0)
		_Gradient("Gradient", 2D) = "white" {}
		_GradientStrength("GradientStrength", Float) = 0
		_color_int("color_int", Float) = 2.14
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#if defined(SHADER_API_D3D11) || defined(SHADER_API_XBOXONE) || defined(UNITY_COMPILER_HLSLCC) || defined(SHADER_API_PSSL) || (defined(SHADER_TARGET_SURFACE_ANALYSIS) && !defined(SHADER_TARGET_SURFACE_ANALYSIS_MOJOSHADER))//ASE Sampler Macros
		#define SAMPLE_TEXTURE2D(tex,samplerTex,coord) tex.Sample(samplerTex,coord)
		#define SAMPLE_TEXTURE2D_LOD(tex,samplerTex,coord,lod) tex.SampleLevel(samplerTex,coord, lod)
		#define SAMPLE_TEXTURE2D_BIAS(tex,samplerTex,coord,bias) tex.SampleBias(samplerTex,coord,bias)
		#define SAMPLE_TEXTURE2D_GRAD(tex,samplerTex,coord,ddx,ddy) tex.SampleGrad(samplerTex,coord,ddx,ddy)
		#else//ASE Sampling Macros
		#define SAMPLE_TEXTURE2D(tex,samplerTex,coord) tex2D(tex,coord)
		#define SAMPLE_TEXTURE2D_LOD(tex,samplerTex,coord,lod) tex2Dlod(tex,float4(coord,0,lod))
		#define SAMPLE_TEXTURE2D_BIAS(tex,samplerTex,coord,bias) tex2Dbias(tex,float4(coord,0,bias))
		#define SAMPLE_TEXTURE2D_GRAD(tex,samplerTex,coord,ddx,ddy) tex2Dgrad(tex,coord,ddx,ddy)
		#endif//ASE Sampling Macros

		#pragma surface surf Unlit alpha:fade keepalpha dithercrossfade 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform half4 _Color;
		uniform half4 _Color0;
		uniform half _GradientStrength;
		UNITY_DECLARE_TEX2D_NOSAMPLER(_Gradient);
		SamplerState sampler_Gradient;
		uniform half4 _Gradient_ST;
		uniform half _color_int;


		half3 ACESTonemap104( half3 linearcolor )
		{
			float a = 2.51f;
			float b = 0.03f;
			float c = 2.43f;
			float d = 0.59f;
			float e = 0.14f;
			return 
			saturate((linearcolor*(a*linearcolor+b))/(linearcolor*(c*linearcolor+d)+e));
		}


		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float2 uv_Gradient = i.uv_texcoord * _Gradient_ST.xy + _Gradient_ST.zw;
			half smoothstepResult100 = smoothstep( 0.0 , _GradientStrength , SAMPLE_TEXTURE2D( _Gradient, sampler_Gradient, uv_Gradient ).r);
			half4 lerpResult95 = lerp( _Color , _Color0 , smoothstepResult100);
			half3 linearcolor104 = ( lerpResult95 * _color_int ).rgb;
			half3 localACESTonemap104 = ACESTonemap104( linearcolor104 );
			o.Emission = localACESTonemap104;
			o.Alpha = smoothstepResult100;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18400
2560;-87;1440;1512;463.9208;332.6919;1.10486;True;True
Node;AmplifyShaderEditor.SamplerNode;74;-814.9752,685.6987;Inherit;True;Property;_Gradient;Gradient;2;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;99;-732.3481,1012.563;Inherit;False;Property;_GradientStrength;GradientStrength;3;0;Create;True;0;0;False;0;False;0;1.49;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;5;-668.8293,126.2228;Inherit;False;Property;_Color;Color ;0;0;Create;True;0;0;False;0;False;0,0,0,0;0.130162,0.2848043,0.3679245,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;96;-670.439,309.5181;Inherit;False;Property;_Color0;Color 0;1;0;Create;True;0;0;False;0;False;0,0,0,0;0.4093984,0.4809328,0.754717,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SmoothstepOpNode;100;-399.7719,715.2501;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;95;-201.439,395.5181;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0.6886792,0.6886792,0.6886792,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;102;-153.7199,593.4684;Inherit;False;Property;_color_int;color_int;4;0;Create;True;0;0;False;0;False;2.14;2.02;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;101;56.85884,379.8804;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.CustomExpressionNode;104;353.6755,491.5334;Inherit;False;float a = 2.51f@$float b = 0.03f@$float c = 2.43f@$float d = 0.59f@$float e = 0.14f@$return $saturate((linearcolor*(a*linearcolor+b))/(linearcolor*(c*linearcolor+d)+e))@;3;False;1;True;linearcolor;FLOAT3;0,0,0;In;;Inherit;False;ACESTonemap;True;False;0;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;642.288,617.4;Half;False;True;-1;2;ASEMaterialInspector;0;0;Unlit;CircleFog;False;False;False;False;False;False;False;False;False;False;False;False;True;False;True;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;False;0;False;Transparent;;Transparent;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;True;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;100;0;74;1
WireConnection;100;2;99;0
WireConnection;95;0;5;0
WireConnection;95;1;96;0
WireConnection;95;2;100;0
WireConnection;101;0;95;0
WireConnection;101;1;102;0
WireConnection;104;0;101;0
WireConnection;0;2;104;0
WireConnection;0;9;100;0
ASEEND*/
//CHKSM=8969FDB44E4A65F9718FF7D62722311AAE7F7E76