// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "bud/PBR"
{
	Properties
	{
		_MainTex("MainTex", 2D) = "white" {}
		_Normal("Normal", 2D) = "bump" {}
		_mra_tex("mra_tex", 2D) = "white" {}
		_Emisson("Emisson", 2D) = "white" {}
		_Emisson_int("Emisson_int", Float) = 0
		[HDR]_Emisson_Color("Emisson_Color", Color) = (1,1,1,0)
		[Toggle(_MATALLIC_OFFON_ON)] _Matallic_offon("Matallic_off/on", Float) = 0
		[Toggle(_SMOOTHNESS_OFFON_ON)] _Smoothness_offon("Smoothness_off/on", Float) = 0
		[Toggle(_AO_OFFON_ON)] _AO_offon("AO_off/on", Float) = 0
		_metalic("metalic", Float) = 0
		_Smoothness("Smoothness", Float) = 0
		_AO("AO", Float) = 0
		_MainTex_Color("MainTex_Color", Color) = (1,1,1,0)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma shader_feature_local _MATALLIC_OFFON_ON
		#pragma shader_feature_local _SMOOTHNESS_OFFON_ON
		#pragma shader_feature_local _AO_OFFON_ON
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _Normal;
		uniform half4 _Normal_ST;
		uniform sampler2D _MainTex;
		uniform half4 _MainTex_ST;
		uniform half4 _MainTex_Color;
		uniform sampler2D _Emisson;
		uniform half4 _Emisson_ST;
		uniform half4 _Emisson_Color;
		uniform half _Emisson_int;
		uniform sampler2D _mra_tex;
		SamplerState sampler_mra_tex;
		uniform half4 _mra_tex_ST;
		uniform half _metalic;
		uniform half _Smoothness;
		uniform half _AO;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_Normal = i.uv_texcoord * _Normal_ST.xy + _Normal_ST.zw;
			o.Normal = UnpackNormal( tex2D( _Normal, uv_Normal ) );
			float2 uv_MainTex = i.uv_texcoord * _MainTex_ST.xy + _MainTex_ST.zw;
			o.Albedo = ( tex2D( _MainTex, uv_MainTex ) * half4( (_MainTex_Color).rgb , 0.0 ) ).rgb;
			float2 uv_Emisson = i.uv_texcoord * _Emisson_ST.xy + _Emisson_ST.zw;
			o.Emission = ( ( tex2D( _Emisson, uv_Emisson ) * _Emisson_Color ) * _Emisson_int ).rgb;
			float2 uv_mra_tex = i.uv_texcoord * _mra_tex_ST.xy + _mra_tex_ST.zw;
			half4 tex2DNode3 = tex2D( _mra_tex, uv_mra_tex );
			#ifdef _MATALLIC_OFFON_ON
				half staticSwitch12 = ( tex2DNode3.r * _metalic );
			#else
				half staticSwitch12 = tex2DNode3.r;
			#endif
			o.Metallic = staticSwitch12;
			#ifdef _SMOOTHNESS_OFFON_ON
				half staticSwitch15 = _Smoothness;
			#else
				half staticSwitch15 = ( 1.0 - tex2DNode3.g );
			#endif
			o.Smoothness = staticSwitch15;
			#ifdef _AO_OFFON_ON
				half staticSwitch17 = _AO;
			#else
				half staticSwitch17 = tex2DNode3.b;
			#endif
			o.Occlusion = staticSwitch17;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18400
716;271;1440;815;1877.235;680.9186;2.489449;True;True
Node;AmplifyShaderEditor.SamplerNode;3;-681.6988,379.4969;Inherit;True;Property;_mra_tex;mra_tex;2;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;13;-683.0071,640.3544;Inherit;True;Property;_metalic;metalic;9;0;Create;True;0;0;False;0;False;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;4;-1286.415,123.3809;Inherit;True;Property;_Emisson;Emisson;3;0;Create;True;0;0;False;0;False;-1;None;9bca2076f745df744b4cc40c48b0910b;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;9;-1205.408,318.0687;Inherit;False;Property;_Emisson_Color;Emisson_Color;5;1;[HDR];Create;True;0;0;False;0;False;1,1,1,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;20;-589.1895,-188.0737;Inherit;False;Property;_MainTex_Color;MainTex_Color;12;0;Create;True;0;0;False;0;False;1,1,1,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;1;-677.0444,-400.7031;Inherit;True;Property;_MainTex;MainTex;0;0;Create;True;0;0;False;0;False;-1;None;25a17d889e03ae142ba909622cd7907d;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;18;-282.6118,1003.813;Inherit;False;Property;_AO;AO;11;0;Create;True;0;0;False;0;False;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;7;-910.923,165.7449;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;16;-277.6118,924.8124;Inherit;False;Property;_Smoothness;Smoothness;10;0;Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;11;-282.3371,693.0008;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;14;-313.0071,461.3544;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SwizzleNode;21;-273.6589,-117.9767;Inherit;False;FLOAT3;0;1;2;3;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;8;-916.7355,329.8353;Inherit;False;Property;_Emisson_int;Emisson_int;4;0;Create;True;0;0;False;0;False;0;1.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;15;63.99292,479.3544;Inherit;False;Property;_Smoothness_offon;Smoothness_off/on;7;0;Create;True;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;12;-76.70711,314.6545;Inherit;True;Property;_Matallic_offon;Matallic_off/on;6;0;Create;True;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;10;-621.29,257.2345;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;2;-667,-8.5;Inherit;True;Property;_Normal;Normal;1;0;Create;True;0;0;False;0;False;-1;None;None;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StaticSwitch;17;68.99292,718.3544;Inherit;False;Property;_AO_offon;AO_off/on;8;0;Create;True;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;19;108.4796,-22.68213;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;405.1671,237.3049;Half;False;True;-1;2;ASEMaterialInspector;0;0;Standard;bud/PBR;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;7;0;4;0
WireConnection;7;1;9;0
WireConnection;11;0;3;2
WireConnection;14;0;3;1
WireConnection;14;1;13;0
WireConnection;21;0;20;0
WireConnection;15;1;11;0
WireConnection;15;0;16;0
WireConnection;12;1;3;1
WireConnection;12;0;14;0
WireConnection;10;0;7;0
WireConnection;10;1;8;0
WireConnection;17;1;3;3
WireConnection;17;0;18;0
WireConnection;19;0;1;0
WireConnection;19;1;21;0
WireConnection;0;0;19;0
WireConnection;0;1;2;0
WireConnection;0;2;10;0
WireConnection;0;3;12;0
WireConnection;0;4;15;0
WireConnection;0;5;17;0
ASEEND*/
//CHKSM=EE67F07516B979192C6F6C63028053F83E97531B