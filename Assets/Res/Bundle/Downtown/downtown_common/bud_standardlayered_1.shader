// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "bud/standardlayered"
{
	Properties
	{
		_Color("Main Color", Color) = (0,0,0,0)
		_MainTex("Albedo", 2D) = "white" {}
		_BumpMap("Normal", 2D) = "bump" {}
		_NormalPower("Normal Power", Float) = 1
		_MetallicGlossMap("Metallic (R) Occlusion (G) Smoothness (A)", 2D) = "white" {}
		_MetallicPower("Metallic Power", Float) = 0
		_SmoothnessPower("Smoothness Power", Float) = 0
		_OcclusionPower("Occlusion Power", Float) = 0
		_SecondAlbedo("Albedo", 2D) = "white" {}
		_DetailNormalMap1("Normal", 2D) = "white" {}
		_SecondNormalPower("Normal Power", Float) = 1
		_DetailNormalMap2("Metallic (R) Occlusion (G) Smoothness (A)", 2D) = "white" {}
		_Tiling2("Tiling", Float) = 0
		[Toggle]_UseVertexColor("Use Vertex Color", Float) = 0
		[Toggle(_VERTEXCOLORCHANNEL_ON)] _VertexColorChannel("Vertex Color Channel", Float) = 0
		_LayerPower("Layer Power", Float) = 0
		_LayerThreshold("Layer Threshold", Float) = 0
		_LayerPosition("Layer Position", Float) = 0
		_LayerContrast("Layer Contrast", Float) = 0
		[Toggle]_BlendNormals("Blend Normals", Float) = 0
		_2ndColor("Color", Color) = (0,0,0,0)
		_DetailAlbedoMap("Albedo", 2D) = "white" {}
		_DetailNormalMap("Normal", 2D) = "white" {}
		_2ndNormalPower("Normal Power", Float) = 1
		_DetailMetallicGlossMap("Metallic (R) Occlusion (G) Smoothness (A)", 2D) = "white" {}
		_Tiling("Tiling", Float) = 0
		[Toggle(_SEEVERTEXCOLORS_ON)] _SeeVertexColors("See Vertex Colors", Float) = 0
		[Toggle(_SMOOTHNESSOFFON_ON)] _Smoothnessoffon("Smoothnessoff/on", Float) = 0
		[Toggle(_METALLICOFFON_ON)] _Metallicoffon("Metallicoff/on", Float) = 0
		[Toggle(_OCCLUSIONOFFON_ON)] _Occlusionoffon("Occlusionoff/on", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGINCLUDE
		#include "UnityStandardUtils.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 5.0
		#pragma shader_feature_local _VERTEXCOLORCHANNEL_ON
		#pragma shader_feature_local _SEEVERTEXCOLORS_ON
		#pragma shader_feature_local _METALLICOFFON_ON
		#pragma shader_feature_local _SMOOTHNESSOFFON_ON
		#pragma shader_feature_local _OCCLUSIONOFFON_ON
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
			float2 uv_texcoord;
			float3 worldPos;
			float3 worldNormal;
			INTERNAL_DATA
			float4 vertexColor : COLOR;
		};

		uniform float _BlendNormals;
		uniform sampler2D _BumpMap;
		uniform float4 _BumpMap_ST;
		uniform float _NormalPower;
		uniform sampler2D _DetailNormalMap1;
		uniform float _Tiling2;
		uniform float _SecondNormalPower;
		uniform sampler2D _DetailNormalMap;
		uniform float _Tiling;
		uniform float _2ndNormalPower;
		uniform float _UseVertexColor;
		uniform float _LayerContrast;
		uniform float _LayerPosition;
		uniform float _LayerPower;
		uniform float _LayerThreshold;
		uniform float4 _Color;
		uniform sampler2D _MainTex;
		uniform float4 _MainTex_ST;
		uniform sampler2D _SecondAlbedo;
		uniform float4 _2ndColor;
		uniform sampler2D _DetailAlbedoMap;
		uniform sampler2D _MetallicGlossMap;
		SamplerState sampler_MetallicGlossMap;
		uniform float4 _MetallicGlossMap_ST;
		uniform sampler2D _DetailNormalMap2;
		uniform sampler2D _DetailMetallicGlossMap;
		uniform float _MetallicPower;
		uniform float _SmoothnessPower;
		uniform float _OcclusionPower;


		inline float3 TriplanarSampling150( sampler2D topTexMap, float3 worldPos, float3 worldNormal, float falloff, float2 tiling, float3 normalScale, float3 index )
		{
			float3 projNormal = ( pow( abs( worldNormal ), falloff ) );
			projNormal /= ( projNormal.x + projNormal.y + projNormal.z ) + 0.00001;
			float3 nsign = sign( worldNormal );
			half4 xNorm; half4 yNorm; half4 zNorm;
			xNorm = tex2D( topTexMap, tiling * worldPos.zy * float2(  nsign.x, 1.0 ) );
			yNorm = tex2D( topTexMap, tiling * worldPos.xz * float2(  nsign.y, 1.0 ) );
			zNorm = tex2D( topTexMap, tiling * worldPos.xy * float2( -nsign.z, 1.0 ) );
			xNorm.xyz  = half3( UnpackScaleNormal( xNorm, normalScale.y ).xy * float2(  nsign.x, 1.0 ) + worldNormal.zy, worldNormal.x ).zyx;
			yNorm.xyz  = half3( UnpackScaleNormal( yNorm, normalScale.x ).xy * float2(  nsign.y, 1.0 ) + worldNormal.xz, worldNormal.y ).xzy;
			zNorm.xyz  = half3( UnpackScaleNormal( zNorm, normalScale.y ).xy * float2( -nsign.z, 1.0 ) + worldNormal.xy, worldNormal.z ).xyz;
			return normalize( xNorm.xyz * projNormal.x + yNorm.xyz * projNormal.y + zNorm.xyz * projNormal.z );
		}


		inline float3 TriplanarSampling63( sampler2D topTexMap, float3 worldPos, float3 worldNormal, float falloff, float2 tiling, float3 normalScale, float3 index )
		{
			float3 projNormal = ( pow( abs( worldNormal ), falloff ) );
			projNormal /= ( projNormal.x + projNormal.y + projNormal.z ) + 0.00001;
			float3 nsign = sign( worldNormal );
			half4 xNorm; half4 yNorm; half4 zNorm;
			xNorm = tex2D( topTexMap, tiling * worldPos.zy * float2(  nsign.x, 1.0 ) );
			yNorm = tex2D( topTexMap, tiling * worldPos.xz * float2(  nsign.y, 1.0 ) );
			zNorm = tex2D( topTexMap, tiling * worldPos.xy * float2( -nsign.z, 1.0 ) );
			xNorm.xyz  = half3( UnpackScaleNormal( xNorm, normalScale.y ).xy * float2(  nsign.x, 1.0 ) + worldNormal.zy, worldNormal.x ).zyx;
			yNorm.xyz  = half3( UnpackScaleNormal( yNorm, normalScale.x ).xy * float2(  nsign.y, 1.0 ) + worldNormal.xz, worldNormal.y ).xzy;
			zNorm.xyz  = half3( UnpackScaleNormal( zNorm, normalScale.y ).xy * float2( -nsign.z, 1.0 ) + worldNormal.xy, worldNormal.z ).xyz;
			return normalize( xNorm.xyz * projNormal.x + yNorm.xyz * projNormal.y + zNorm.xyz * projNormal.z );
		}


		float4 CalculateContrast( float contrastValue, float4 colorTarget )
		{
			float t = 0.5 * ( 1.0 - contrastValue );
			return mul( float4x4( contrastValue,0,0,t, 0,contrastValue,0,t, 0,0,contrastValue,t, 0,0,0,1 ), colorTarget );
		}

		inline float4 TriplanarSampling148( sampler2D topTexMap, float3 worldPos, float3 worldNormal, float falloff, float2 tiling, float3 normalScale, float3 index )
		{
			float3 projNormal = ( pow( abs( worldNormal ), falloff ) );
			projNormal /= ( projNormal.x + projNormal.y + projNormal.z ) + 0.00001;
			float3 nsign = sign( worldNormal );
			half4 xNorm; half4 yNorm; half4 zNorm;
			xNorm = tex2D( topTexMap, tiling * worldPos.zy * float2(  nsign.x, 1.0 ) );
			yNorm = tex2D( topTexMap, tiling * worldPos.xz * float2(  nsign.y, 1.0 ) );
			zNorm = tex2D( topTexMap, tiling * worldPos.xy * float2( -nsign.z, 1.0 ) );
			return xNorm * projNormal.x + yNorm * projNormal.y + zNorm * projNormal.z;
		}


		inline float4 TriplanarSampling60( sampler2D topTexMap, float3 worldPos, float3 worldNormal, float falloff, float2 tiling, float3 normalScale, float3 index )
		{
			float3 projNormal = ( pow( abs( worldNormal ), falloff ) );
			projNormal /= ( projNormal.x + projNormal.y + projNormal.z ) + 0.00001;
			float3 nsign = sign( worldNormal );
			half4 xNorm; half4 yNorm; half4 zNorm;
			xNorm = tex2D( topTexMap, tiling * worldPos.zy * float2(  nsign.x, 1.0 ) );
			yNorm = tex2D( topTexMap, tiling * worldPos.xz * float2(  nsign.y, 1.0 ) );
			zNorm = tex2D( topTexMap, tiling * worldPos.xy * float2( -nsign.z, 1.0 ) );
			return xNorm * projNormal.x + yNorm * projNormal.y + zNorm * projNormal.z;
		}


		float3 ACESTonemap178( float3 linearcolor )
		{
			float a = 2.51f;
			float b = 0.03f;
			float c = 2.43f;
			float d = 0.59f;
			float e = 0.14f;
			return 
			saturate((linearcolor*(a*linearcolor+b))/(linearcolor*(c*linearcolor+d)+e));
		}


		inline float4 TriplanarSampling152( sampler2D topTexMap, float3 worldPos, float3 worldNormal, float falloff, float2 tiling, float3 normalScale, float3 index )
		{
			float3 projNormal = ( pow( abs( worldNormal ), falloff ) );
			projNormal /= ( projNormal.x + projNormal.y + projNormal.z ) + 0.00001;
			float3 nsign = sign( worldNormal );
			half4 xNorm; half4 yNorm; half4 zNorm;
			xNorm = tex2D( topTexMap, tiling * worldPos.zy * float2(  nsign.x, 1.0 ) );
			yNorm = tex2D( topTexMap, tiling * worldPos.xz * float2(  nsign.y, 1.0 ) );
			zNorm = tex2D( topTexMap, tiling * worldPos.xy * float2( -nsign.z, 1.0 ) );
			return xNorm * projNormal.x + yNorm * projNormal.y + zNorm * projNormal.z;
		}


		inline float4 TriplanarSampling65( sampler2D topTexMap, float3 worldPos, float3 worldNormal, float falloff, float2 tiling, float3 normalScale, float3 index )
		{
			float3 projNormal = ( pow( abs( worldNormal ), falloff ) );
			projNormal /= ( projNormal.x + projNormal.y + projNormal.z ) + 0.00001;
			float3 nsign = sign( worldNormal );
			half4 xNorm; half4 yNorm; half4 zNorm;
			xNorm = tex2D( topTexMap, tiling * worldPos.zy * float2(  nsign.x, 1.0 ) );
			yNorm = tex2D( topTexMap, tiling * worldPos.xz * float2(  nsign.y, 1.0 ) );
			zNorm = tex2D( topTexMap, tiling * worldPos.xy * float2( -nsign.z, 1.0 ) );
			return xNorm * projNormal.x + yNorm * projNormal.y + zNorm * projNormal.z;
		}


		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_BumpMap = i.uv_texcoord * _BumpMap_ST.xy + _BumpMap_ST.zw;
			float2 appendResult146 = (float2(_Tiling2 , _Tiling2));
			float2 WorldSpaceSecondMaps147 = appendResult146;
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float3 ase_worldTangent = WorldNormalVector( i, float3( 1, 0, 0 ) );
			float3 ase_worldBitangent = WorldNormalVector( i, float3( 0, 1, 0 ) );
			float3x3 ase_worldToTangent = float3x3( ase_worldTangent, ase_worldBitangent, ase_worldNormal );
			float3 triplanar150 = TriplanarSampling150( _DetailNormalMap1, ase_worldPos, ase_worldNormal, 1.0, WorldSpaceSecondMaps147, _SecondNormalPower, 0 );
			float3 tanTriplanarNormal150 = mul( ase_worldToTangent, triplanar150 );
			float3 temp_output_138_0 = BlendNormals( UnpackScaleNormal( tex2D( _BumpMap, uv_BumpMap ), _NormalPower ) , tanTriplanarNormal150 );
			float2 appendResult71 = (float2(_Tiling , _Tiling));
			float2 WorldSpaceDeposit48 = appendResult71;
			float3 triplanar63 = TriplanarSampling63( _DetailNormalMap, ase_worldPos, ase_worldNormal, 1.0, WorldSpaceDeposit48, _2ndNormalPower, 0 );
			float3 tanTriplanarNormal63 = mul( ase_worldToTangent, triplanar63 );
			float4 temp_cast_0 = ((WorldNormalVector( i , tanTriplanarNormal63 )).y).xxxx;
			#ifdef _VERTEXCOLORCHANNEL_ON
				float staticSwitch162 = i.vertexColor.g;
			#else
				float staticSwitch162 = i.vertexColor.r;
			#endif
			float4 temp_cast_1 = (pow( staticSwitch162 , _LayerPosition )).xxxx;
			float4 clampResult105 = clamp( CalculateContrast(_LayerContrast,temp_cast_1) , float4( 0,0,0,0 ) , float4( 1,1,1,0 ) );
			float4 temp_cast_2 = (( 1.0 - _LayerPower )).xxxx;
			float4 temp_cast_3 = ((0.001 + (_LayerThreshold - 0.0) * (1.0 - 0.001) / (1.0 - 0.0))).xxxx;
			float4 BlendAlpha85 = pow( saturate( ( (( _UseVertexColor )?( ( pow( clampResult105 , temp_cast_2 ) * clampResult105 ) ):( temp_cast_0 )) + _LayerPower ) ) , temp_cast_3 );
			float3 lerpResult13 = lerp( temp_output_138_0 , tanTriplanarNormal63 , BlendAlpha85.rgb);
			float4 color81 = IsGammaSpace() ? float4(0,0,0,0) : float4(0,0,0,0);
			float4 lerpResult78 = lerp( color81 , float4( tanTriplanarNormal63 , 0.0 ) , BlendAlpha85);
			float3 Normal118 = (( _BlendNormals )?( BlendNormals( temp_output_138_0 , lerpResult78.rgb ) ):( lerpResult13 ));
			o.Normal = Normal118;
			float2 uv_MainTex = i.uv_texcoord * _MainTex_ST.xy + _MainTex_ST.zw;
			float4 triplanar148 = TriplanarSampling148( _SecondAlbedo, ase_worldPos, ase_worldNormal, 1.0, WorldSpaceSecondMaps147, 1.0, 0 );
			float4 triplanar60 = TriplanarSampling60( _DetailAlbedoMap, ase_worldPos, ase_worldNormal, 1.0, WorldSpaceDeposit48, 1.0, 0 );
			float4 lerpResult26 = lerp( ( _Color * ( tex2D( _MainTex, uv_MainTex ) * triplanar148 ) ) , ( _2ndColor * triplanar60 ) , BlendAlpha85);
			#ifdef _SEEVERTEXCOLORS_ON
				float4 staticSwitch165 = i.vertexColor;
			#else
				float4 staticSwitch165 = lerpResult26;
			#endif
			float4 Albedo116 = staticSwitch165;
			float3 linearcolor178 = ( Albedo116 * Albedo116 ).rgb;
			float3 localACESTonemap178 = ACESTonemap178( linearcolor178 );
			o.Albedo = sqrt( localACESTonemap178 );
			float2 uv_MetallicGlossMap = i.uv_texcoord * _MetallicGlossMap_ST.xy + _MetallicGlossMap_ST.zw;
			float4 tex2DNode7 = tex2D( _MetallicGlossMap, uv_MetallicGlossMap );
			float4 triplanar152 = TriplanarSampling152( _DetailNormalMap2, ase_worldPos, ase_worldNormal, 1.0, WorldSpaceSecondMaps147, 1.0, 0 );
			float4 triplanar65 = TriplanarSampling65( _DetailMetallicGlossMap, ase_worldPos, ase_worldNormal, 1.0, WorldSpaceDeposit48, 1.0, 0 );
			float lerpResult30 = lerp( ( tex2DNode7.r * triplanar152.x ) , triplanar65.x , BlendAlpha85.r);
			#ifdef _METALLICOFFON_ON
				float staticSwitch181 = _MetallicPower;
			#else
				float staticSwitch181 = ( lerpResult30 + _MetallicPower );
			#endif
			float Metallic120 = staticSwitch181;
			o.Metallic = Metallic120;
			float lerpResult31 = lerp( ( tex2DNode7.a * triplanar152.w ) , triplanar65.w , BlendAlpha85.r);
			#ifdef _SMOOTHNESSOFFON_ON
				float staticSwitch182 = _SmoothnessPower;
			#else
				float staticSwitch182 = ( lerpResult31 * _SmoothnessPower );
			#endif
			float Smoothness121 = staticSwitch182;
			o.Smoothness = Smoothness121;
			float lerpResult33 = lerp( ( tex2DNode7.g * triplanar152.y ) , triplanar65.y , BlendAlpha85.r);
			#ifdef _OCCLUSIONOFFON_ON
				float staticSwitch183 = 0.0;
			#else
				float staticSwitch183 = pow( lerpResult33 , _OcclusionPower );
			#endif
			float Occlusion122 = staticSwitch183;
			o.Occlusion = Occlusion122;
			o.Alpha = 1;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard keepalpha fullforwardshadows 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 5.0
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
				half4 color : COLOR0;
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
				o.color = v.color;
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
				surfIN.vertexColor = IN.color;
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
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
}
/*ASEBEGIN
Version=18400
2560;-87;1440;1512;3182.949;4161.147;4.92562;True;True
Node;AmplifyShaderEditor.CommentaryNode;115;-4224,384;Inherit;False;3409.094;635.7684;Deposit Mask;19;85;24;17;25;16;35;14;93;72;105;73;112;22;113;109;34;110;162;163;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;114;-3584,-2176;Inherit;False;604;191;World-Space UVs - Deposit;3;48;71;54;;1,1,1,1;0;0
Node;AmplifyShaderEditor.VertexColorNode;34;-4144,640;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StaticSwitch;162;-3968,640;Inherit;False;Property;_VertexColorChannel;Vertex Color Channel;14;0;Create;True;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;110;-3968,832;Inherit;False;Property;_LayerPosition;Layer Position;17;0;Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;54;-3536,-2112;Inherit;False;Property;_Tiling;Tiling;25;0;Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;71;-3376,-2112;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;113;-3712,896;Inherit;False;Property;_LayerContrast;Layer Contrast;18;0;Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;109;-3626.05,684.562;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;48;-3232,-2112;Inherit;False;WorldSpaceDeposit;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;128;-4228.933,-743.3338;Inherit;False;2687.375;1026.049;Normals;19;118;76;3;150;63;52;9;64;141;139;8;151;75;13;78;87;138;81;86;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleContrastOpNode;112;-3456,784;Inherit;False;2;1;COLOR;0,0,0,0;False;0;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;22;-3328,480;Float;False;Property;_LayerPower;Layer Power;15;0;Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;105;-3200,784;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;9;-3972.933,-167.3338;Inherit;False;Property;_2ndNormalPower;Normal Power;23;0;Create;False;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;64;-4196.933,-263.3337;Inherit;True;Property;_DetailNormalMap;Normal;22;0;Create;False;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.GetLocalVarNode;52;-3972.933,-39.33375;Inherit;False;48;WorldSpaceDeposit;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.OneMinusNode;73;-3040,432;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;72;-2848,592;Inherit;True;False;2;0;COLOR;0,0,0,0;False;1;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.TriplanarNode;63;-3588.933,-231.3338;Inherit;True;Spherical;World;True;Top Texture 1;_TopTexture1;white;-1;None;Mid Texture 1;_MidTexture1;white;-1;None;Bot Texture 1;_BotTexture1;white;-1;None;Triplanar Sampler;Tangent;10;0;SAMPLER2D;;False;5;FLOAT;1;False;1;SAMPLER2D;;False;6;FLOAT;0;False;2;SAMPLER2D;;False;7;FLOAT;0;False;9;FLOAT3;0,0,0;False;8;FLOAT;1;False;3;FLOAT2;1,1;False;4;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;144;-4223.918,-2173.562;Inherit;False;610;189;World-Space UVs - Second Maps;3;147;146;145;;1,1,1,1;0;0
Node;AmplifyShaderEditor.WorldNormalVector;14;-2336,544;Inherit;True;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;93;-2592,752;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;145;-4175.918,-2109.562;Inherit;False;Property;_Tiling2;Tiling;12;0;Create;False;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ToggleSwitchNode;35;-1984,734;Inherit;False;Property;_UseVertexColor;Use Vertex Color;13;0;Create;True;0;0;False;0;False;0;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.DynamicAppendNode;146;-4015.918,-2109.562;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;147;-3871.918,-2109.562;Inherit;False;WorldSpaceSecondMaps;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;129;-4219.236,-1900.945;Inherit;False;1990.061;1061.969;Diffuse / Colors;17;116;164;165;26;12;88;10;11;134;2;60;1;148;61;49;140;149;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;25;-1823,834;Float;False;Property;_LayerThreshold;Layer Threshold;16;0;Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;16;-1728,464;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.TexturePropertyNode;149;-4155.236,-1532.945;Inherit;True;Property;_SecondAlbedo;Albedo;8;0;Create;False;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.GetLocalVarNode;140;-4187.236,-1340.945;Inherit;False;147;WorldSpaceSecondMaps;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TFHCRemapNode;163;-1508.247,823.5432;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0.001;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;17;-1504,464;Inherit;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TexturePropertyNode;61;-3963.236,-1212.945;Inherit;True;Property;_DetailAlbedoMap;Albedo;21;0;Create;False;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.TriplanarNode;148;-3899.236,-1484.945;Inherit;True;Spherical;World;False;Top Texture 5;_TopTexture5;white;-1;None;Mid Texture 5;_MidTexture5;white;-1;None;Bot Texture 5;_BotTexture5;white;-1;None;Triplanar Sampler;Tangent;10;0;SAMPLER2D;;False;5;FLOAT;1;False;1;SAMPLER2D;;False;6;FLOAT;0;False;2;SAMPLER2D;;False;7;FLOAT;0;False;9;FLOAT3;0,0,0;False;8;FLOAT;1;False;3;FLOAT2;1,1;False;4;FLOAT;1;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;1;-3835.236,-1724.945;Inherit;True;Property;_MainTex;Albedo;1;0;Create;False;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;49;-3963.236,-956.9453;Inherit;False;48;WorldSpaceDeposit;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PowerNode;24;-1312,464;Inherit;True;False;2;0;COLOR;0,0,0,0;False;1;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;127;-4224,1152;Inherit;False;2301.409;1022.124;Metallic / Smoothness / Occlusion;26;120;121;122;30;31;33;136;65;135;137;90;7;66;50;152;142;153;166;170;169;172;174;171;183;181;182;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;134;-3451.236,-1596.945;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;11;-3451.236,-1340.945;Inherit;False;Property;_2ndColor;Color;20;0;Create;False;0;0;False;0;False;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;85;-1024,512;Inherit;False;BlendAlpha;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;2;-3451.236,-1852.945;Inherit;False;Property;_Color;Main Color;0;0;Create;False;0;0;False;0;False;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TriplanarNode;60;-3579.236,-1084.945;Inherit;True;Spherical;World;False;Top Texture 0;_TopTexture0;white;-1;None;Mid Texture 0;_MidTexture0;white;-1;None;Bot Texture 0;_BotTexture0;white;-1;None;Triplanar Sampler;Tangent;10;0;SAMPLER2D;;False;5;FLOAT;1;False;1;SAMPLER2D;;False;6;FLOAT;0;False;2;SAMPLER2D;;False;7;FLOAT;0;False;9;FLOAT3;0,0,0;False;8;FLOAT;1;False;3;FLOAT2;1,1;False;4;FLOAT;1;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;10;-3163.236,-1596.945;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;88;-3163.236,-1340.945;Inherit;False;85;BlendAlpha;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;12;-3163.236,-1468.945;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;142;-4096,1632;Inherit;False;147;WorldSpaceSecondMaps;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TexturePropertyNode;153;-4160,1408;Inherit;True;Property;_DetailNormalMap2;Metallic (R) Occlusion (G) Smoothness (A);11;0;Create;False;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.GetLocalVarNode;50;-4096,2016;Inherit;False;48;WorldSpaceDeposit;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TexturePropertyNode;66;-4160,1792;Inherit;True;Property;_DetailMetallicGlossMap;Metallic (R) Occlusion (G) Smoothness (A);24;0;Create;False;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.TriplanarNode;152;-3791,1458;Inherit;True;Spherical;World;False;Top Texture 7;_TopTexture7;white;-1;None;Mid Texture 7;_MidTexture7;white;-1;None;Bot Texture 7;_BotTexture7;white;-1;None;Triplanar Sampler;Tangent;10;0;SAMPLER2D;;False;5;FLOAT;1;False;1;SAMPLER2D;;False;6;FLOAT;0;False;2;SAMPLER2D;;False;7;FLOAT;0;False;9;FLOAT3;0,0,0;False;8;FLOAT;1;False;3;FLOAT2;1,1;False;4;FLOAT;1;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;7;-3712,1200;Inherit;True;Property;_MetallicGlossMap;Metallic (R) Occlusion (G) Smoothness (A);4;0;Create;False;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;139;-3972.933,-551.3338;Inherit;False;Property;_SecondNormalPower;Normal Power;10;0;Create;False;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;151;-4194.933,-485.3337;Inherit;True;Property;_DetailNormalMap1;Normal;9;0;Create;False;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.VertexColorNode;164;-2859.236,-1260.945;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;141;-3972.933,-423.3337;Inherit;False;147;WorldSpaceSecondMaps;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.LerpOp;26;-2939.236,-1532.945;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;8;-3972.933,-679.3338;Inherit;False;Property;_NormalPower;Normal Power;3;0;Create;True;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;165;-2683.236,-1388.945;Inherit;False;Property;_SeeVertexColors;See Vertex Colors;26;0;Create;True;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;90;-3072,1984;Inherit;False;85;BlendAlpha;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;137;-3328,1456;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;136;-3328,1232;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TriplanarNode;65;-3792,1888;Inherit;True;Spherical;World;False;Top Texture 2;_TopTexture2;white;-1;None;Mid Texture 2;_MidTexture2;white;-1;None;Bot Texture 2;_BotTexture2;white;-1;None;Triplanar Sampler;Tangent;10;0;SAMPLER2D;;False;5;FLOAT;1;False;1;SAMPLER2D;;False;6;FLOAT;0;False;2;SAMPLER2D;;False;7;FLOAT;0;False;9;FLOAT3;0,0,0;False;8;FLOAT;1;False;3;FLOAT2;1,1;False;4;FLOAT;1;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;86;-2948.933,-39.33375;Inherit;False;85;BlendAlpha;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;3;-3588.933,-679.3338;Inherit;True;Property;_BumpMap;Normal;2;0;Create;False;0;0;False;0;False;-1;None;None;True;0;False;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TriplanarNode;150;-3588.933,-487.3337;Inherit;True;Spherical;World;True;Top Texture 6;_TopTexture6;white;-1;None;Mid Texture 6;_MidTexture6;white;-1;None;Bot Texture 6;_BotTexture6;white;-1;None;Triplanar Sampler;Tangent;10;0;SAMPLER2D;;False;5;FLOAT;1;False;1;SAMPLER2D;;False;6;FLOAT;0;False;2;SAMPLER2D;;False;7;FLOAT;0;False;9;FLOAT3;0,0,0;False;8;FLOAT;1;False;3;FLOAT2;1,1;False;4;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;81;-2948.933,-295.3337;Inherit;False;Constant;_Color0;Color 0;22;0;Create;True;0;0;False;0;False;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;135;-3328,1728;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;170;-2565.2,1360.9;Inherit;False;Property;_MetallicPower;Metallic Power;5;0;Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;31;-2816,1472;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;169;-2560,1600;Inherit;False;Property;_SmoothnessPower;Smoothness Power;6;0;Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;33;-2816,1728;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;30;-2816,1216;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.BlendNormalsNode;138;-3124.933,-567.3338;Inherit;False;0;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;78;-2692.933,-167.3338;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;87;-2692.933,-551.3338;Inherit;False;85;BlendAlpha;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;166;-2560,1856;Inherit;False;Property;_OcclusionPower;Occlusion Power;7;0;Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;116;-2427.236,-1388.945;Inherit;False;Albedo;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;117;-902.2603,-379.7919;Inherit;False;116;Albedo;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.BlendNormalsNode;75;-2436.933,-199.3338;Inherit;True;0;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.PowerNode;174;-2410.598,1726.973;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;172;-2415.701,1474.6;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;171;-2422,1208;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;13;-2436.933,-679.3338;Inherit;True;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StaticSwitch;182;-2295.875,1466.37;Inherit;False;Property;_Smoothnessoffon;Smoothnessoff/on;27;0;Create;True;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;181;-2306.275,1229.77;Inherit;False;Property;_Metallicoffon;Metallicoff/on;28;0;Create;True;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;183;-2283.975,1730.27;Inherit;False;Property;_Occlusionoffon;Occlusionoff/on;29;0;Create;True;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ToggleSwitchNode;76;-2052.933,-455.3337;Inherit;False;Property;_BlendNormals;Blend Normals;19;0;Create;True;0;0;False;0;False;0;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;176;-500.8604,-401.7983;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;120;-2122,1215;Inherit;False;Metallic;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;121;-2100.098,1468.1;Inherit;False;Smoothness;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;122;-2123.1,1730;Inherit;False;Occlusion;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CustomExpressionNode;178;-222.3488,-374.9224;Inherit;False;float a = 2.51f@$float b = 0.03f@$float c = 2.43f@$float d = 0.59f@$float e = 0.14f@$return $saturate((linearcolor*(a*linearcolor+b))/(linearcolor*(c*linearcolor+d)+e))@;3;False;1;True;linearcolor;FLOAT3;0,0,0;In;;Inherit;False;ACESTonemap;True;False;0;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;118;-1796.933,-423.3337;Inherit;False;Normal;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;125;-256,128;Inherit;False;122;Occlusion;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;124;-256,32;Inherit;False;121;Smoothness;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;119;-256,-160;Inherit;False;118;Normal;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;123;-256,-64;Inherit;False;120;Metallic;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SqrtOpNode;177;20.46897,-377.4396;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;254.0535,-96.96348;Float;False;True;-1;7;;0;0;Standard;bud/standardlayered;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;162;1;34;1
WireConnection;162;0;34;2
WireConnection;71;0;54;0
WireConnection;71;1;54;0
WireConnection;109;0;162;0
WireConnection;109;1;110;0
WireConnection;48;0;71;0
WireConnection;112;1;109;0
WireConnection;112;0;113;0
WireConnection;105;0;112;0
WireConnection;73;0;22;0
WireConnection;72;0;105;0
WireConnection;72;1;73;0
WireConnection;63;0;64;0
WireConnection;63;8;9;0
WireConnection;63;3;52;0
WireConnection;14;0;63;0
WireConnection;93;0;72;0
WireConnection;93;1;105;0
WireConnection;35;0;14;2
WireConnection;35;1;93;0
WireConnection;146;0;145;0
WireConnection;146;1;145;0
WireConnection;147;0;146;0
WireConnection;16;0;35;0
WireConnection;16;1;22;0
WireConnection;163;0;25;0
WireConnection;17;0;16;0
WireConnection;148;0;149;0
WireConnection;148;3;140;0
WireConnection;24;0;17;0
WireConnection;24;1;163;0
WireConnection;134;0;1;0
WireConnection;134;1;148;0
WireConnection;85;0;24;0
WireConnection;60;0;61;0
WireConnection;60;3;49;0
WireConnection;10;0;2;0
WireConnection;10;1;134;0
WireConnection;12;0;11;0
WireConnection;12;1;60;0
WireConnection;152;0;153;0
WireConnection;152;3;142;0
WireConnection;26;0;10;0
WireConnection;26;1;12;0
WireConnection;26;2;88;0
WireConnection;165;1;26;0
WireConnection;165;0;164;0
WireConnection;137;0;7;4
WireConnection;137;1;152;4
WireConnection;136;0;7;1
WireConnection;136;1;152;1
WireConnection;65;0;66;0
WireConnection;65;3;50;0
WireConnection;3;5;8;0
WireConnection;150;0;151;0
WireConnection;150;8;139;0
WireConnection;150;3;141;0
WireConnection;135;0;7;2
WireConnection;135;1;152;2
WireConnection;31;0;137;0
WireConnection;31;1;65;4
WireConnection;31;2;90;0
WireConnection;33;0;135;0
WireConnection;33;1;65;2
WireConnection;33;2;90;0
WireConnection;30;0;136;0
WireConnection;30;1;65;1
WireConnection;30;2;90;0
WireConnection;138;0;3;0
WireConnection;138;1;150;0
WireConnection;78;0;81;0
WireConnection;78;1;63;0
WireConnection;78;2;86;0
WireConnection;116;0;165;0
WireConnection;75;0;138;0
WireConnection;75;1;78;0
WireConnection;174;0;33;0
WireConnection;174;1;166;0
WireConnection;172;0;31;0
WireConnection;172;1;169;0
WireConnection;171;0;30;0
WireConnection;171;1;170;0
WireConnection;13;0;138;0
WireConnection;13;1;63;0
WireConnection;13;2;87;0
WireConnection;182;1;172;0
WireConnection;182;0;169;0
WireConnection;181;1;171;0
WireConnection;181;0;170;0
WireConnection;183;1;174;0
WireConnection;76;0;13;0
WireConnection;76;1;75;0
WireConnection;176;0;117;0
WireConnection;176;1;117;0
WireConnection;120;0;181;0
WireConnection;121;0;182;0
WireConnection;122;0;183;0
WireConnection;178;0;176;0
WireConnection;118;0;76;0
WireConnection;177;0;178;0
WireConnection;0;0;177;0
WireConnection;0;1;119;0
WireConnection;0;3;123;0
WireConnection;0;4;124;0
WireConnection;0;5;125;0
ASEEND*/
//CHKSM=87E70F7AA769BB923FBE2DDAB1F1B53C0FA99198