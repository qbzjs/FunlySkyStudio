// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "FarPlaneFog"
{
	Properties
	{
		_FogMask("FogMask", 2D) = "white" {}
		_FogTex("FogTex", 2D) = "white" {}
		_FogColor("FogColor", Color) = (1,1,1,0)
		_FLowSpeed("FLowSpeed", Range( 0 , 0.1)) = 0.1
		_FogTex2("FogTex2", 2D) = "white" {}
		_FogTex3("FogTex3", 2D) = "white" {}
		_FogIntensity("FogIntensity", Range( 0 , 15)) = 0
		_TimeOffset("TimeOffset", Range( 0 , 1)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#define ASE_USING_SAMPLING_MACROS 1
		#if defined(SHADER_API_D3D11) || defined(SHADER_API_XBOXONE) || defined(UNITY_COMPILER_HLSLCC) || defined(SHADER_API_PSSL) || (defined(SHADER_TARGET_SURFACE_ANALYSIS) && !defined(SHADER_TARGET_SURFACE_ANALYSIS_MOJOSHADER))//ASE Sampler Macros
		#define SAMPLE_TEXTURE2D(tex,samplerTex,coord) tex.Sample(samplerTex,coord)
		#else//ASE Sampling Macros
		#define SAMPLE_TEXTURE2D(tex,samplerTex,coord) tex2D(tex,coord)
		#endif//ASE Sampling Macros

		struct Input
		{
			float2 uv_texcoord;
		};

		uniform half4 _FogColor;
		UNITY_DECLARE_TEX2D_NOSAMPLER(_FogTex);
		uniform half _FLowSpeed;
		uniform half _TimeOffset;
		SamplerState sampler_FogTex;
		UNITY_DECLARE_TEX2D_NOSAMPLER(_FogTex2);
		SamplerState sampler_FogTex2;
		UNITY_DECLARE_TEX2D_NOSAMPLER(_FogTex3);
		uniform half4 _FogTex3_ST;
		SamplerState sampler_FogTex3;
		uniform half _FogIntensity;
		UNITY_DECLARE_TEX2D_NOSAMPLER(_FogMask);
		uniform half4 _FogMask_ST;
		SamplerState sampler_FogMask;

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			o.Emission = _FogColor.rgb;
			half mulTime62 = _Time.y * _FLowSpeed;
			half temp_output_86_0 = ( mulTime62 + _TimeOffset );
			half2 appendResult64 = (half2(( temp_output_86_0 * 0.75 ) , 0.32));
			float2 uv_TexCoord4 = i.uv_texcoord * float2( 0.45,0.29 ) + appendResult64;
			half2 appendResult80 = (half2(( ( temp_output_86_0 * 1.35 ) + -0.4 ) , 0.29));
			float2 uv_TexCoord70 = i.uv_texcoord * float2( 0.58,0.32 ) + appendResult80;
			float2 uv_FogTex3 = i.uv_texcoord * _FogTex3_ST.xy + _FogTex3_ST.zw;
			float2 uv_FogMask = i.uv_texcoord * _FogMask_ST.xy + _FogMask_ST.zw;
			half clampResult79 = clamp( ( ( 1.0 - pow( ( 1.0 - ( max( SAMPLE_TEXTURE2D( _FogTex, sampler_FogTex, uv_TexCoord4 ).r , SAMPLE_TEXTURE2D( _FogTex2, sampler_FogTex2, uv_TexCoord70 ).r ) * SAMPLE_TEXTURE2D( _FogTex3, sampler_FogTex3, uv_FogTex3 ).r ) ) , _FogIntensity ) ) * SAMPLE_TEXTURE2D( _FogMask, sampler_FogMask, uv_FogMask ).r ) , 0.0 , 1.0 );
			o.Alpha = clampResult79;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Unlit alpha:fade keepalpha fullforwardshadows 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			sampler3D _DitherMaskLOD;
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float3 worldPos : TEXCOORD2;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				o.worldPos = worldPos;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				float3 worldPos = IN.worldPos;
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				SurfaceOutput o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutput, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				half alphaRef = tex3D( _DitherMaskLOD, float3( vpos.xy * 0.25, o.Alpha * 0.9375 ) ).a;
				clip( alphaRef - 0.01 );
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18912
2561;271;2062;1072;-182.8618;499.113;1;True;False
Node;AmplifyShaderEditor.RangedFloatNode;63;-1880.123,-148.2114;Inherit;False;Property;_FLowSpeed;FLowSpeed;3;0;Create;True;0;0;0;False;0;False;0.1;0.0339;0;0.1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;85;-1674.359,-34.03229;Inherit;False;Property;_TimeOffset;TimeOffset;7;0;Create;True;0;0;0;False;0;False;0;0.201;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;62;-1565.476,-144.0209;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;86;-1248.359,-134.0323;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;65;-901.2994,26.4178;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;1.35;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;82;-735.0203,25.59967;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;-0.4;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;66;-733.1226,-141.2114;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.75;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;80;-570.0203,26.59967;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0.29;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;64;-556.2996,-140.7964;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0.32;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;70;-402.9353,-19.80504;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;0.58,0.32;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;4;-404.4477,-189.298;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;0.45,0.29;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;2;-159.9672,-276.241;Inherit;True;Property;_FogTex;FogTex;1;0;Create;True;0;0;0;False;0;False;2;b72814b88d9ed0540b2f36ff0459e12d;b72814b88d9ed0540b2f36ff0459e12d;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;69;-164.0653,-76.24884;Inherit;True;Property;_FogTex2;FogTex2;4;0;Create;True;0;0;0;False;0;False;69;None;b72814b88d9ed0540b2f36ff0459e12d;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMaxOpNode;7;272.478,-176.7008;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;74;-157.0375,140.0995;Inherit;True;Property;_FogTex3;FogTex3;5;0;Create;True;0;0;0;False;0;False;74;None;d0c23af59cedc744285499b1890d1ca5;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;71;644.6392,-149.436;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;88;804.8618,-101.113;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;83;305.2084,17.33472;Inherit;False;Property;_FogIntensity;FogIntensity;6;0;Create;True;0;0;0;False;0;False;0;8.53;0;15;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;87;948.8618,-73.11304;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;1;-130.2948,361.1071;Inherit;True;Property;_FogMask;FogMask;0;0;Create;True;0;0;0;False;0;False;1;7e058835f9f7b3a4699e4f99701f8147;7e058835f9f7b3a4699e4f99701f8147;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;89;1069.862,-37.11304;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;61;1163.038,-41.0676;Inherit;False;2;2;0;FLOAT;1;False;1;FLOAT;3.4;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;79;1406.642,-129.993;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;10;911.2205,-353.5083;Inherit;False;Property;_FogColor;FogColor;2;0;Create;True;0;0;0;False;0;False;1,1,1,0;0.3336353,0.5579429,0.8117647,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1667.1,-335.7;Half;False;True;-1;2;ASEMaterialInspector;0;0;Unlit;FarPlaneFog;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;True;0;False;Transparent;;Transparent;All;18;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;2;5;False;-1;10;False;-1;2;5;False;-1;10;False;-1;1;False;-1;3;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;True;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;62;0;63;0
WireConnection;86;0;62;0
WireConnection;86;1;85;0
WireConnection;65;0;86;0
WireConnection;82;0;65;0
WireConnection;66;0;86;0
WireConnection;80;0;82;0
WireConnection;64;0;66;0
WireConnection;70;1;80;0
WireConnection;4;1;64;0
WireConnection;2;1;4;0
WireConnection;69;1;70;0
WireConnection;7;0;2;1
WireConnection;7;1;69;1
WireConnection;71;0;7;0
WireConnection;71;1;74;1
WireConnection;88;0;71;0
WireConnection;87;0;88;0
WireConnection;87;1;83;0
WireConnection;89;0;87;0
WireConnection;61;0;89;0
WireConnection;61;1;1;1
WireConnection;79;0;61;0
WireConnection;0;2;10;0
WireConnection;0;9;79;0
ASEEND*/
//CHKSM=39449A06F9913D76963E29F0F701010EDBC29792