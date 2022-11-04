// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "ordinary/trrain_normal"
{
	Properties
	{
		_SpecularColor("Specular Color", Color) = (0.3921569,0.3921569,0.3921569,1)
		_Shininess("Shininess", Range( 0.01 , 1)) = 0.1
		_fog_intensity("fog_intensity", Range( 0 , 10)) = 0
		_contrast("contrast", Range( 0 , 5)) = 0
		_fogcontrast("fogcontrast", Range( 0 , 50)) = 0
		_r1("r1", Range( 0 , 1)) = 0
		_r2("r2", Range( 0 , 1)) = 0
		_Fog_Color("Fog_Color", Color) = (0,0,0,0)
		_Fog_Color2("Fog_Color2", Color) = (0,0,0,0)
		_fog_distance_start("fog_distance_start", Float) = 0
		_fog_distance_end("fog_distance_end", Float) = 0
		_fog_heighr_start("fog_heighr_start", Float) = 0
		_fog_heighr_end("fog_heighr_end", Float) = 0
		_Normal("Normal", 2D) = "white" {}
		_BaseColor("BaseColor", 2D) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Background+0" }
		Cull Back
		ZWrite On
		ZTest LEqual
		CGINCLUDE
		#include "UnityPBSLighting.cginc"
		#include "UnityCG.cginc"
		#include "UnityShaderVariables.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#define ASE_USING_SAMPLING_MACROS 1
		#if defined(SHADER_API_D3D11) || defined(SHADER_API_XBOXONE) || defined(UNITY_COMPILER_HLSLCC) || defined(SHADER_API_PSSL) || (defined(SHADER_TARGET_SURFACE_ANALYSIS) && !defined(SHADER_TARGET_SURFACE_ANALYSIS_MOJOSHADER))//ASE Sampler Macros
		#define SAMPLE_TEXTURE2D(tex,samplerTex,coord) tex.Sample(samplerTex,coord)
		#else//ASE Sampling Macros
		#define SAMPLE_TEXTURE2D(tex,samplerTex,coord) tex2D(tex,coord)
		#endif//ASE Sampling Macros

		#ifdef UNITY_PASS_SHADOWCASTER
			#undef INTERNAL_DATA
			#undef WorldReflectionVector
			#undef WorldNormalVector
			#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
			#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
			#define WorldNormalVector(data,normal) half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
		#endif
		struct Input
		{
			float3 worldPos;
			half3 worldNormal;
			INTERNAL_DATA
			float2 uv_texcoord;
		};

		struct SurfaceOutputCustomLightingCustom
		{
			half3 Albedo;
			half3 Normal;
			half3 Emission;
			half Metallic;
			half Smoothness;
			half Occlusion;
			half Alpha;
			Input SurfInput;
			UnityGIInput GIData;
		};

		uniform half _contrast;
		uniform float4 _SpecularColor;
		UNITY_DECLARE_TEX2D_NOSAMPLER(_Normal);
		uniform half4 _Normal_ST;
		SamplerState sampler_Normal;
		uniform float _Shininess;
		UNITY_DECLARE_TEX2D_NOSAMPLER(_BaseColor);
		uniform half4 _BaseColor_ST;
		SamplerState sampler_BaseColor;
		uniform half4 _Fog_Color;
		uniform half4 _Fog_Color2;
		uniform half _r1;
		uniform half _r2;
		uniform half _fog_heighr_end;
		uniform half _fog_heighr_start;
		uniform half _fog_distance_end;
		uniform half _fog_distance_start;
		uniform half _fog_intensity;
		uniform half _fogcontrast;


		float4 CalculateContrast( float contrastValue, float4 colorTarget )
		{
			float t = 0.5 * ( 1.0 - contrastValue );
			return mul( float4x4( contrastValue,0,0,t, 0,contrastValue,0,t, 0,0,contrastValue,t, 0,0,0,1 ), colorTarget );
		}

		inline half4 LightingStandardCustomLighting( inout SurfaceOutputCustomLightingCustom s, half3 viewDir, UnityGI gi )
		{
			UnityGIInput data = s.GIData;
			Input i = s.SurfInput;
			half4 c = 0;
			#ifdef UNITY_PASS_FORWARDBASE
			float ase_lightAtten = data.atten;
			if( _LightColor0.a == 0)
			ase_lightAtten = 0;
			#else
			float3 ase_lightAttenRGB = gi.light.color / ( ( _LightColor0.rgb ) + 0.000001 );
			float ase_lightAtten = max( max( ase_lightAttenRGB.r, ase_lightAttenRGB.g ), ase_lightAttenRGB.b );
			#endif
			#if defined(HANDLE_SHADOWS_BLENDING_IN_GI)
			half bakedAtten = UnitySampleBakedOcclusion(data.lightmapUV.xy, data.worldPos);
			float zDist = dot(_WorldSpaceCameraPos - data.worldPos, UNITY_MATRIX_V[2].xyz);
			float fadeDist = UnityComputeShadowFadeDistance(data.worldPos, zDist);
			ase_lightAtten = UnityMixRealtimeAndBakedShadows(data.atten, bakedAtten, UnityComputeShadowFade(fadeDist));
			#endif
			half4 temp_output_43_0_g5 = _SpecularColor;
			float3 ase_worldPos = i.worldPos;
			half3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			#if defined(LIGHTMAP_ON) && UNITY_VERSION < 560 //aseld
			half3 ase_worldlightDir = 0;
			#else //aseld
			half3 ase_worldlightDir = normalize( UnityWorldSpaceLightDir( ase_worldPos ) );
			#endif //aseld
			half3 normalizeResult4_g6 = normalize( ( ase_worldViewDir + ase_worldlightDir ) );
			float2 uv_Normal = i.uv_texcoord * _Normal_ST.xy + _Normal_ST.zw;
			half3 normalizeResult64_g5 = normalize( (WorldNormalVector( i , SAMPLE_TEXTURE2D( _Normal, sampler_Normal, uv_Normal ).rgb )) );
			half dotResult19_g5 = dot( normalizeResult4_g6 , normalizeResult64_g5 );
			#if defined(LIGHTMAP_ON) && ( UNITY_VERSION < 560 || ( defined(LIGHTMAP_SHADOW_MIXING) && !defined(SHADOWS_SHADOWMASK) && defined(SHADOWS_SCREEN) ) )//aselc
			half4 ase_lightColor = 0;
			#else //aselc
			half4 ase_lightColor = _LightColor0;
			#endif //aselc
			half4 temp_output_40_0_g5 = ( ase_lightColor * ase_lightAtten );
			half dotResult14_g5 = dot( normalizeResult64_g5 , ase_worldlightDir );
			UnityGI gi34_g5 = gi;
			float3 diffNorm34_g5 = normalizeResult64_g5;
			gi34_g5 = UnityGI_Base( data, 1, diffNorm34_g5 );
			half3 indirectDiffuse34_g5 = gi34_g5.indirect.diffuse + diffNorm34_g5 * 0.0001;
			float2 uv_BaseColor = i.uv_texcoord * _BaseColor_ST.xy + _BaseColor_ST.zw;
			half4 tex2DNode79 = SAMPLE_TEXTURE2D( _BaseColor, sampler_BaseColor, uv_BaseColor );
			half4 temp_output_42_0_g5 = tex2DNode79;
			half4 temp_output_90_0 = CalculateContrast(_contrast,( ( half4( (temp_output_43_0_g5).rgb , 0.0 ) * (temp_output_43_0_g5).a * pow( max( dotResult19_g5 , 0.0 ) , ( _Shininess * 128.0 ) ) * temp_output_40_0_g5 ) + ( ( ( temp_output_40_0_g5 * max( dotResult14_g5 , 0.0 ) ) + half4( indirectDiffuse34_g5 , 0.0 ) ) * half4( (temp_output_42_0_g5).rgb , 0.0 ) ) ));
			half temp_output_12_0_g3 = _fog_heighr_end;
			half clampResult8_g3 = clamp( ( ( temp_output_12_0_g3 - ase_worldPos.y ) / ( temp_output_12_0_g3 - _fog_heighr_start ) ) , 0.0 , 1.0 );
			half fog_heighr36 = ( 1.0 - ( 1.0 - clampResult8_g3 ) );
			half temp_output_117_0 = pow( fog_heighr36 , 0.5 );
			half temp_output_12_0_g4 = _fog_distance_end;
			half clampResult8_g4 = clamp( ( ( temp_output_12_0_g4 - distance( ase_worldPos , _WorldSpaceCameraPos ) ) / ( temp_output_12_0_g4 - _fog_distance_start ) ) , 0.0 , 1.0 );
			half fogdistance27 = ( 1.0 - clampResult8_g4 );
			half temp_output_118_0 = pow( fogdistance27 , 0.5 );
			half clampResult40 = clamp( ( ( temp_output_117_0 * temp_output_118_0 ) * _fog_intensity ) , 0.0 , 1.0 );
			half lerpResult98 = lerp( _r1 , _r2 , clampResult40);
			half4 lerpResult105 = lerp( _Fog_Color , _Fog_Color2 , lerpResult98);
			half4 temp_cast_4 = (clampResult40).xxxx;
			half4 lerpResult16 = lerp( temp_output_90_0 , lerpResult105 , CalculateContrast(_fogcontrast,temp_cast_4));
			c.rgb = lerpResult16.rgb;
			c.a = 1;
			return c;
		}

		inline void LightingStandardCustomLighting_GI( inout SurfaceOutputCustomLightingCustom s, UnityGIInput data, inout UnityGI gi )
		{
			s.GIData = data;
		}

		void surf( Input i , inout SurfaceOutputCustomLightingCustom o )
		{
			o.SurfInput = i;
			o.Normal = float3(0,0,1);
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf StandardCustomLighting keepalpha fullforwardshadows noshadow 

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
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float4 tSpace0 : TEXCOORD2;
				float4 tSpace1 : TEXCOORD3;
				float4 tSpace2 : TEXCOORD4;
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
				half3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
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
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
				SurfaceOutputCustomLightingCustom o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputCustomLightingCustom, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
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
2561;271;2062;1072;587.2153;175.1976;1.3;True;False
Node;AmplifyShaderEditor.CommentaryNode;25;-3012.742,260.5888;Inherit;False;1467.899;572.9387;fog distance;7;27;24;10;8;11;7;6;fog distance;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;29;-3008.948,996.5435;Inherit;False;1467.899;572.9387;fog heighr;6;36;35;34;32;31;37;fog heighr;1,1,1,1;0;0
Node;AmplifyShaderEditor.WorldSpaceCameraPos;7;-2962.742,534.3728;Inherit;True;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldPosInputsNode;31;-2933.867,1046.544;Inherit;True;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldPosInputsNode;6;-2937.661,310.5889;Inherit;True;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;34;-2669.122,1253.143;Inherit;False;Property;_fog_heighr_start;fog_heighr_start;14;0;Create;True;0;0;0;False;0;False;0;-214;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;32;-2663.443,1326.009;Inherit;False;Property;_fog_heighr_end;fog_heighr_end;15;0;Create;True;0;0;0;False;0;False;0;2036.15;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;10;-2355.731,705.1868;Inherit;False;Property;_fog_distance_end;fog_distance_end;13;0;Create;True;0;0;0;False;0;False;0;7721;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;35;-2388.652,1247.78;Inherit;True;Fog_Linear;-1;;3;e7ccb771044eb844a82ad80f3883fb27;0;3;14;FLOAT;500;False;13;FLOAT;700;False;12;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DistanceOpNode;8;-2585.358,391.1328;Inherit;True;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;11;-2342.41,624.0209;Inherit;False;Property;_fog_distance_start;fog_distance_start;12;0;Create;True;0;0;0;False;0;False;0;185.1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;24;-2056.447,471.8252;Inherit;True;Fog_Linear;-1;;4;e7ccb771044eb844a82ad80f3883fb27;0;3;14;FLOAT;500;False;13;FLOAT;700;False;12;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;37;-2004.706,1240.217;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;36;-1764.028,1203.985;Inherit;False;fog_heighr;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;27;-1767.823,468.0309;Inherit;False;fogdistance;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;42;-1377.185,993.689;Inherit;False;27;fogdistance;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;38;-1372,901.5706;Inherit;False;36;fog_heighr;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;117;-1118.133,915.1124;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;118;-1104.133,1003.112;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;66;-1056.654,1189.035;Inherit;False;Property;_fog_intensity;fog_intensity;5;0;Create;True;0;0;0;False;0;False;0;1.91;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;41;-826.2259,1030.787;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;65;-663.67,1028.324;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;40;-456.1243,1022.871;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;101;-656.8723,638.3134;Inherit;False;Property;_r1;r1;8;0;Create;True;0;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;79;-1918.582,-41.12144;Inherit;True;Property;_BaseColor;BaseColor;19;0;Create;True;0;0;0;False;0;False;-1;None;69494ea684752c541aaa8959e2b314f3;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;102;-633.6022,719.1857;Inherit;False;Property;_r2;r2;9;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;69;-916.1695,268.0651;Inherit;True;Property;_Normal;Normal;16;0;Create;True;0;0;0;False;0;False;-1;None;21fc5259ba2aab14ca5ab5289ede6252;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;92;-326.9228,250.3006;Inherit;False;Blinn-Phong Light;1;;5;cf814dba44d007a4e958d2ddd5813da6;0;3;42;COLOR;0,0,0,0;False;52;FLOAT3;0,0,0;False;43;COLOR;0,0,0,0;False;2;COLOR;0;FLOAT;57
Node;AmplifyShaderEditor.RangedFloatNode;91;-645.0815,486.3864;Inherit;False;Property;_contrast;contrast;6;0;Create;True;0;0;0;False;0;False;0;0.31;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;98;-132.9969,853.2999;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;104;-482.0278,1154.89;Inherit;False;Property;_fogcontrast;fogcontrast;7;0;Create;True;0;0;0;False;0;False;0;0.9;0;50;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;17;-94.93817,468.3619;Inherit;False;Property;_Fog_Color;Fog_Color;10;0;Create;True;0;0;0;False;0;False;0,0,0,0;0.1122285,0.1692519,0.2452829,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;106;-186.6147,659.2722;Inherit;False;Property;_Fog_Color2;Fog_Color2;11;0;Create;True;0;0;0;False;0;False;0,0,0,0;0.1041295,0.1484738,0.1886792,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleContrastOpNode;90;102.7254,222.304;Inherit;False;2;1;COLOR;0,0,0,0;False;0;FLOAT;0.39;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;105;233.1844,480.7791;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleContrastOpNode;103;573.7347,1087.188;Inherit;False;2;1;COLOR;0,0,0,0;False;0;FLOAT;0.39;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;94;-425.5268,78.03973;Inherit;False;Constant;_Float0;Float 0;11;0;Create;True;0;0;0;False;0;False;-0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;95;427.4214,133.4245;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;16;972.9988,524.903;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;109;-1431.47,-137.1453;Half;False;Property;_Color0;Color 0;17;0;Create;True;0;0;0;False;0;False;0.1121396,0.1511897,0.2264151,0;0.09531856,0.1933342,0.3207546,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMinOpNode;97;-931.3912,888.7931;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Compare;114;-1358.195,161.5184;Inherit;False;5;4;0;FLOAT;0;False;1;FLOAT;0;False;2;COLOR;1,1,1,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;93;-198.5268,8.46225;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;107;-715.7834,74.23842;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;83;-457.6638,-104.9702;Inherit;False;Property;_Color1;Color 1;20;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;111;-1537.16,189.3243;Inherit;False;Property;_f1f;f1f;18;0;Create;True;0;0;0;False;0;False;0;0.71;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;96;-62.43164,71.31287;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0.5,0.5,0.5,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1389.383,329.382;Half;False;True;-1;2;ASEMaterialInspector;0;0;CustomLighting;ordinary/trrain_normal;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;1;False;-1;3;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;True;0;True;Opaque;;Background;All;18;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;0;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;0;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;True;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
Node;AmplifyShaderEditor.CommentaryNode;115;-1105.658,1476.647;Inherit;False;100;100;Comment;0;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;116;-727.3577,1511.933;Inherit;False;100;100;Comment;0;;1,1,1,1;0;0
WireConnection;35;14;31;2
WireConnection;35;13;34;0
WireConnection;35;12;32;0
WireConnection;8;0;6;0
WireConnection;8;1;7;0
WireConnection;24;14;8;0
WireConnection;24;13;11;0
WireConnection;24;12;10;0
WireConnection;37;0;35;0
WireConnection;36;0;37;0
WireConnection;27;0;24;0
WireConnection;117;0;38;0
WireConnection;118;0;42;0
WireConnection;41;0;117;0
WireConnection;41;1;118;0
WireConnection;65;0;41;0
WireConnection;65;1;66;0
WireConnection;40;0;65;0
WireConnection;92;42;79;0
WireConnection;92;52;69;0
WireConnection;98;0;101;0
WireConnection;98;1;102;0
WireConnection;98;2;40;0
WireConnection;90;1;92;0
WireConnection;90;0;91;0
WireConnection;105;0;17;0
WireConnection;105;1;106;0
WireConnection;105;2;98;0
WireConnection;103;1;40;0
WireConnection;103;0;104;0
WireConnection;95;0;96;0
WireConnection;95;1;90;0
WireConnection;16;0;90;0
WireConnection;16;1;105;0
WireConnection;16;2;103;0
WireConnection;97;0;117;0
WireConnection;97;1;118;0
WireConnection;114;0;79;1
WireConnection;114;1;111;0
WireConnection;93;0;83;0
WireConnection;93;1;94;0
WireConnection;107;0;109;0
WireConnection;107;1;79;0
WireConnection;107;2;114;0
WireConnection;96;0;93;0
WireConnection;0;13;16;0
ASEEND*/
//CHKSM=0AA59CB2D69CE88228A64CB941167660E64A9285