// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "bud/Water_03"
{
	Properties
	{
		_Metallic("Metallic", Float) = 0
		_Smoothness("Smoothness", Float) = 0
		_opacity("opacity", Float) = 0
		_normalmap("normalmap", 2D) = "white" {}
		_Waves_int("Waves_int", Vector) = (0,1,0,0)
		_SkimeTilling("SkimeTilling", Vector) = (1,1,1,0)
		_SlimeNoiseSpeed_02("SlimeNoiseSpeed_02", Vector) = (0,0,0,0)
		_SlimeNoiseSpeed_01("SlimeNoiseSpeed_01", Vector) = (0,0,0,0)
		_WavesHeight("Waves Height", Float) = 0
		_WavesSpeed("Waves Speed", Float) = 0
		_WavesScale("Waves Scale", Float) = 0
		_Color_02("Color_02", Color) = (1,1,1,0)
		_Color_01("Color_01", Color) = (1,1,1,0)
		_Normalstex_int("Normalstex_int", Float) = 1
		_RimColor("RimColor", Color) = (1,1,1,0)
		_RimBias("RimBias", Float) = 0
		_RimScale("RimScale", Float) = 0
		_RimPower("RimPower", Float) = 5
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" }
		Cull Back
		CGINCLUDE
		#include "UnityPBSLighting.cginc"
		#include "UnityShaderVariables.cginc"
		#include "UnityCG.cginc"
		#include "UnityStandardUtils.cginc"
		#include "Lighting.cginc"
		#pragma target 4.6
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
			float3 worldNormal;
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

		uniform float _WavesSpeed;
		uniform float _WavesScale;
		uniform float _WavesHeight;
		uniform float2 _Waves_int;
		uniform float _opacity;
		uniform float4 _Color_01;
		uniform float4 _Color_02;
		uniform sampler2D _normalmap;
		uniform float3 _SlimeNoiseSpeed_01;
		uniform float3 _SkimeTilling;
		uniform float _Normalstex_int;
		uniform float3 _SlimeNoiseSpeed_02;
		uniform float _RimBias;
		uniform float _RimScale;
		uniform float _RimPower;
		uniform float4 _RimColor;
		uniform float _Metallic;
		uniform float _Smoothness;


		float3 mod2D289( float3 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }

		float2 mod2D289( float2 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }

		float3 permute( float3 x ) { return mod2D289( ( ( x * 34.0 ) + 1.0 ) * x ); }

		float snoise( float2 v )
		{
			const float4 C = float4( 0.211324865405187, 0.366025403784439, -0.577350269189626, 0.024390243902439 );
			float2 i = floor( v + dot( v, C.yy ) );
			float2 x0 = v - i + dot( i, C.xx );
			float2 i1;
			i1 = ( x0.x > x0.y ) ? float2( 1.0, 0.0 ) : float2( 0.0, 1.0 );
			float4 x12 = x0.xyxy + C.xxzz;
			x12.xy -= i1;
			i = mod2D289( i );
			float3 p = permute( permute( i.y + float3( 0.0, i1.y, 1.0 ) ) + i.x + float3( 0.0, i1.x, 1.0 ) );
			float3 m = max( 0.5 - float3( dot( x0, x0 ), dot( x12.xy, x12.xy ), dot( x12.zw, x12.zw ) ), 0.0 );
			m = m * m;
			m = m * m;
			float3 x = 2.0 * frac( p * C.www ) - 1.0;
			float3 h = abs( x ) - 0.5;
			float3 ox = floor( x + 0.5 );
			float3 a0 = x - ox;
			m *= 1.79284291400159 - 0.85373472095314 * ( a0 * a0 + h * h );
			float3 g;
			g.x = a0.x * x0.x + h.x * x0.y;
			g.yz = a0.yz * x12.xz + h.yz * x12.yw;
			return 130.0 * dot( m, g );
		}


		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 ase_vertexNormal = v.normal.xyz;
			float2 appendResult195 = (float2(_WavesSpeed , _WavesSpeed));
			float3 ase_worldPos = mul( unity_ObjectToWorld, v.vertex );
			float2 appendResult194 = (float2(ase_worldPos.x , ase_worldPos.z));
			float2 panner196 = ( 1.0 * _Time.y * appendResult195 + appendResult194);
			float simplePerlin2D199 = snoise( panner196*( _WavesScale / 100.0 ) );
			simplePerlin2D199 = simplePerlin2D199*0.5 + 0.5;
			float3 worldToObjDir273 = mul( unity_WorldToObject, float4( ( ase_vertexNormal * ( simplePerlin2D199 * _WavesHeight ) ), 0 ) ).xyz;
			float3 WavesHeight49 = ( worldToObjDir273 * float3( _Waves_int ,  0.0 ) );
			v.vertex.xyz += WavesHeight49;
		}

		inline half4 LightingStandardCustomLighting( inout SurfaceOutputCustomLightingCustom s, half3 viewDir, UnityGI gi )
		{
			UnityGIInput data = s.GIData;
			Input i = s.SurfInput;
			half4 c = 0;
			SurfaceOutputStandard s388 = (SurfaceOutputStandard ) 0;
			float3 ase_worldPos = i.worldPos;
			#if defined(LIGHTMAP_ON) && UNITY_VERSION < 560 //aseld
			float3 ase_worldlightDir = 0;
			#else //aseld
			float3 ase_worldlightDir = normalize( UnityWorldSpaceLightDir( ase_worldPos ) );
			#endif //aseld
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float dotResult389 = dot( ase_worldlightDir , ase_worldNormal );
			float4 lerpResult396 = lerp( _Color_01 , _Color_02 , dotResult389);
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float mulTime418 = _Time.y * _SlimeNoiseSpeed_01.z;
			float3 objToWorld428 = mul( unity_ObjectToWorld, float4( float3( 0,0,0 ), 1 ) ).xyz;
			float3 temp_output_423_0 = ( ( float3( i.uv_texcoord ,  0.0 ) - objToWorld428 ) * _SkimeTilling );
			float3 tex2DNode383 = UnpackScaleNormal( tex2D( _normalmap, ( ( mulTime418 * _SlimeNoiseSpeed_01 ) + temp_output_423_0 ).xy ), _Normalstex_int );
			float mulTime421 = _Time.y * _SlimeNoiseSpeed_02.z;
			float3 temp_output_430_0 = BlendNormals( tex2DNode383 , UnpackScaleNormal( tex2D( _normalmap, ( temp_output_423_0 + ( mulTime421 * _SlimeNoiseSpeed_02 ) ).xy ), _Normalstex_int ) );
			float3 objToWorld409 = mul( unity_ObjectToWorld, float4( temp_output_430_0, 1 ) ).xyz;
			float fresnelNdotV400 = dot( normalize( objToWorld409 ), ase_worldViewDir );
			float fresnelNode400 = ( _RimBias + _RimScale * pow( 1.0 - fresnelNdotV400, _RimPower ) );
			s388.Albedo = ( lerpResult396 + ( fresnelNode400 * _RimColor ) ).rgb;
			s388.Normal = WorldNormalVector( i , temp_output_430_0 );
			s388.Emission = float3( 0,0,0 );
			s388.Metallic = _Metallic;
			s388.Smoothness = _Smoothness;
			s388.Occlusion = 1.0;

			data.light = gi.light;

			UnityGI gi388 = gi;
			#ifdef UNITY_PASS_FORWARDBASE
			Unity_GlossyEnvironmentData g388 = UnityGlossyEnvironmentSetup( s388.Smoothness, data.worldViewDir, s388.Normal, float3(0,0,0));
			gi388 = UnityGlobalIllumination( data, s388.Occlusion, s388.Normal, g388 );
			#endif

			float3 surfResult388 = LightingStandard ( s388, viewDir, gi388 ).rgb;
			surfResult388 += s388.Emission;

			#ifdef UNITY_PASS_FORWARDADD//388
			surfResult388 -= s388.Emission;
			#endif//388
			c.rgb = surfResult388;
			c.a = _opacity;
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
		#pragma surface surf StandardCustomLighting alpha:fade keepalpha fullforwardshadows vertex:vertexDataFunc 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 4.6
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
				vertexDataFunc( v, customInputData );
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
				UnityGI gi;
				UNITY_INITIALIZE_OUTPUT( UnityGI, gi );
				o.Alpha = LightingStandardCustomLighting( o, worldViewDir, gi ).a;
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
Version=18400
778;517;1440;803;-3710.025;-1088.551;1;True;True
Node;AmplifyShaderEditor.CommentaryNode;164;-670.6141,2074.222;Inherit;False;2172.17;608.8064;Waves;16;49;273;200;201;191;48;199;196;198;194;195;197;193;192;436;435;Waves;1,1,1,1;0;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;416;-854.269,842.4572;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TransformPositionNode;428;-854.269,970.4573;Inherit;False;Object;World;False;Fast;True;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.Vector3Node;417;-867.269,629.4572;Inherit;False;Property;_SlimeNoiseSpeed_01;SlimeNoiseSpeed_01;12;0;Create;True;0;0;False;0;False;0,0,0;0.1,0.1,0.15;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.Vector3Node;415;-852.269,1333.457;Inherit;False;Property;_SlimeNoiseSpeed_02;SlimeNoiseSpeed_02;11;0;Create;True;0;0;False;0;False;0,0,0;0.1,0.1,0.1;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleTimeNode;421;-278.269,1258.457;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;418;-486.269,522.457;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;420;-582.269,922.4572;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.Vector3Node;419;-838.2691,1162.457;Inherit;False;Property;_SkimeTilling;SkimeTilling;10;0;Create;True;0;0;False;0;False;1,1,1;10,10,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;193;-606.6136,2330.221;Inherit;False;Property;_WavesSpeed;Waves Speed;14;0;Create;True;0;0;False;0;False;0;5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;192;-606.6136,2138.222;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;422;89.73094,666.4573;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;423;73.73094,890.4572;Inherit;True;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DynamicAppendNode;195;-382.6143,2314.221;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;194;-414.6143,2170.222;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;424;201.7309,1322.457;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;197;-222.6141,2394.222;Inherit;False;Property;_WavesScale;Waves Scale;15;0;Create;True;0;0;False;0;False;0;4;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;426;560.1187,1044.71;Inherit;True;Property;_normalmap;normalmap;8;0;Create;True;0;0;False;0;False;None;9c5b42a27f5ef2347b3f3930c7fcd5a5;True;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.RangedFloatNode;399;772.7244,1297.954;Inherit;False;Property;_Normalstex_int;Normalstex_int;18;0;Create;True;0;0;False;0;False;1;0.54;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;425;606.9,767.1341;Inherit;True;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;427;592.4387,1420.87;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;198;33.38647,2394.222;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;100;False;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;196;-222.6141,2234.222;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;429;1054.267,1364.574;Inherit;True;Property;_TextureSample2;Texture Sample 2;24;0;Create;True;0;0;False;0;False;-1;None;9c5b42a27f5ef2347b3f3930c7fcd5a5;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;48;-606.6136,2458.222;Inherit;False;Property;_WavesHeight;Waves Height;13;0;Create;True;0;0;False;0;False;0;3;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;383;1018.755,1101.73;Inherit;True;Property;_Normalstex;Normalstex;24;0;Create;True;0;0;False;0;False;-1;None;9c5b42a27f5ef2347b3f3930c7fcd5a5;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.NoiseGeneratorNode;199;193.386,2234.222;Inherit;True;Simplex2D;True;False;2;0;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.BlendNormalsNode;430;1479.269,1082.457;Inherit;False;0;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;191;481.3867,2426.222;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NormalVertexDataNode;200;513.3868,2186.222;Inherit;True;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;402;1631.724,1315.294;Inherit;False;Property;_RimScale;RimScale;21;0;Create;True;0;0;False;0;False;0;0.07;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.WorldNormalVector;386;967.4961,649.0134;Inherit;True;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.TransformPositionNode;409;1730.097,1110.648;Inherit;False;Object;World;False;Fast;True;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;403;1631.004,1387.373;Inherit;False;Property;_RimPower;RimPower;22;0;Create;True;0;0;False;0;False;5;5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;401;1635.724,1243.294;Inherit;False;Property;_RimBias;RimBias;20;0;Create;True;0;0;False;0;False;0;0.03;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;385;1013.488,417.1002;Inherit;True;False;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;201;801.3869,2266.221;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DotProductOpNode;389;1531.955,528.447;Inherit;True;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;390;2022.584,408.1939;Inherit;False;Property;_Color_01;Color_01;17;0;Create;True;0;0;False;0;False;1,1,1,0;0,0.01509435,0.1037736,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;405;1858.378,1494.312;Inherit;False;Property;_RimColor;RimColor;19;0;Create;True;0;0;False;0;False;1,1,1,0;0,0.1992508,0.2075472,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TransformDirectionNode;273;993.3867,2202.221;Inherit;False;World;Object;False;Fast;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.Vector2Node;435;1023.714,2533.952;Inherit;False;Property;_Waves_int;Waves_int;9;0;Create;True;0;0;False;0;False;0,1;0,0.5;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.ColorNode;392;1939.367,673.9478;Inherit;False;Property;_Color_02;Color_02;16;0;Create;True;0;0;False;0;False;1,1,1,0;0,0.5184241,0.764151,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FresnelNode;400;2002.934,1199.197;Inherit;True;Standard;WorldNormal;ViewDir;True;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;436;1197.587,2360.079;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT2;0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;404;2552.837,810.6523;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;396;2318.359,609.9482;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;49;1327.135,2355.279;Inherit;False;WavesHeight;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;407;2650.505,700.6431;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;176;2158.765,1441.044;Inherit;False;Property;_Metallic;Metallic;5;0;Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;13;2152.599,1529.076;Inherit;False;Property;_Smoothness;Smoothness;6;0;Create;True;0;0;False;0;False;0;0.88;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;51;3029.477,1642.894;Inherit;False;49;WavesHeight;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CustomStandardSurface;388;2835.251,790.4485;Inherit;False;Metallic;Tangent;6;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,1;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;408;1523.389,885.8589;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;398;3897.159,832.8697;Inherit;True;Property;_opacity;opacity;7;0;Create;True;0;0;False;0;False;0;0.8;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;379;4501.595,1133.375;Float;False;True;-1;6;ASEMaterialInspector;0;0;CustomLighting;bud/Water_03;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;True;0;False;Transparent;;Transparent;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;50;10;25;False;0.5;True;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;0;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;421;0;415;3
WireConnection;418;0;417;3
WireConnection;420;0;416;0
WireConnection;420;1;428;0
WireConnection;422;0;418;0
WireConnection;422;1;417;0
WireConnection;423;0;420;0
WireConnection;423;1;419;0
WireConnection;195;0;193;0
WireConnection;195;1;193;0
WireConnection;194;0;192;1
WireConnection;194;1;192;3
WireConnection;424;0;421;0
WireConnection;424;1;415;0
WireConnection;425;0;422;0
WireConnection;425;1;423;0
WireConnection;427;0;423;0
WireConnection;427;1;424;0
WireConnection;198;0;197;0
WireConnection;196;0;194;0
WireConnection;196;2;195;0
WireConnection;429;0;426;0
WireConnection;429;1;427;0
WireConnection;429;5;399;0
WireConnection;383;0;426;0
WireConnection;383;1;425;0
WireConnection;383;5;399;0
WireConnection;199;0;196;0
WireConnection;199;1;198;0
WireConnection;430;0;383;0
WireConnection;430;1;429;0
WireConnection;191;0;199;0
WireConnection;191;1;48;0
WireConnection;409;0;430;0
WireConnection;201;0;200;0
WireConnection;201;1;191;0
WireConnection;389;0;385;0
WireConnection;389;1;386;0
WireConnection;273;0;201;0
WireConnection;400;0;409;0
WireConnection;400;1;401;0
WireConnection;400;2;402;0
WireConnection;400;3;403;0
WireConnection;436;0;273;0
WireConnection;436;1;435;0
WireConnection;404;0;400;0
WireConnection;404;1;405;0
WireConnection;396;0;390;0
WireConnection;396;1;392;0
WireConnection;396;2;389;0
WireConnection;49;0;436;0
WireConnection;407;0;396;0
WireConnection;407;1;404;0
WireConnection;388;0;407;0
WireConnection;388;1;430;0
WireConnection;388;3;176;0
WireConnection;388;4;13;0
WireConnection;408;0;386;0
WireConnection;408;1;383;0
WireConnection;379;9;398;0
WireConnection;379;13;388;0
WireConnection;379;11;51;0
ASEEND*/
//CHKSM=58AA4113F03FB0A4C8FA85A4DC7B3AD27BEAA5B4