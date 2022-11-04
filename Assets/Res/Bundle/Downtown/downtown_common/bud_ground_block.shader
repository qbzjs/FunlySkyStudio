// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "bud/ground_block"
{
	Properties
	{
		_Color_01("Color _01", Color) = (0,0,0,0)
		_Color_02("Color _02", Color) = (0,0,0,0)
		_Color_03("Color _03", Color) = (0,0,0,0)
		_Color_04("Color _04", Color) = (0,0,0,0)
		_MainTex_mask("MainTex_mask", 2D) = "white" {}
		_MainTex_01_R("MainTex_01_R", 2D) = "white" {}
		_normal_01("normal_01", 2D) = "bump" {}
		_normal_01_scale("normal_01_scale", Float) = 1
		_MaiTex_01("MaiTex_01", 2D) = "white" {}
		_MaiTex_01_M_int("MaiTex_01_M_int", Float) = 1
		_MaiTex_01_S_int("MaiTex_01_S_int", Float) = 1
		_MaiTex_01_Ao_int("MaiTex_01_Ao_int", Float) = 1
		_MainTex_02_back("MainTex_02_back", 2D) = "white" {}
		_normal_02("normal_02", 2D) = "bump" {}
		_normal_02_scale("normal_02_scale", Float) = 1
		_MaTex_02("MaTex_02", 2D) = "white" {}
		_MaiTex_02_M_int("MaiTex_02_M_int", Float) = 1
		_MaiTex_02_S_int("MaiTex_02_S_int", Float) = 1
		_MaiTex_02_Ao_int("MaiTex_02_Ao_int", Float) = 1
		_MainTex_03_G("MainTex_03_G", 2D) = "white" {}
		_normal_03("normal_03", 2D) = "bump" {}
		_normal_03_scale("normal_03_scale", Float) = 1
		_MaTex_03("MaTex_03", 2D) = "white" {}
		_MaiTex_03_M_int("MaiTex_03_M_int", Float) = 1
		_MaiTex_03_S_int("MaiTex_03_S_int", Float) = 1
		_MaiTex_03_Ao_int("MaiTex_03_Ao_int", Float) = 1
		_MainTex_04_B("MainTex_04_B", 2D) = "white" {}
		_normal_04("normal_04", 2D) = "bump" {}
		_normal_04_scale("normal_04_scale", Float) = 1
		_MaTex_04("MaTex_04", 2D) = "white" {}
		_MaiTex_04_M_int("MaiTex_04_M_int", Float) = 1
		_MaiTex_04_S_int("MaiTex_04_S_int", Float) = 1
		_MaiTex_04_Ao_int("MaiTex_04_Ao_int", Float) = 1
		_normal_05_2u3u("normal_05_2u/3u", 2D) = "bump" {}
		_normal_05_scale("normal_05_scale", Float) = 1
		[Toggle(_AO_ONOFF_ON)] _AO_onoff("AO_on/off", Float) = 0
		[Toggle(_NORMAL_05_2U3U_ON)] _normal_05_2u3u("normal_05_2u/3u", Float) = 0
		[HideInInspector] _texcoord3( "", 2D ) = "white" {}
		[HideInInspector] _texcoord2( "", 2D ) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IgnoreProjector" = "True" }
		Cull Back
		Blend SrcAlpha OneMinusSrcAlpha
		
		CGPROGRAM
		#include "UnityStandardUtils.cginc"
		#pragma target 3.5
		#pragma shader_feature_local _NORMAL_05_2U3U_ON
		#pragma shader_feature_local _AO_ONOFF_ON
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
			float2 uv2_texcoord2;
			float2 uv3_texcoord3;
		};

		uniform sampler2D _normal_01;
		uniform float4 _normal_01_ST;
		uniform float _normal_01_scale;
		uniform sampler2D _MainTex_mask;
		uniform float4 _MainTex_mask_ST;
		uniform sampler2D _normal_02;
		uniform float4 _normal_02_ST;
		uniform float _normal_02_scale;
		uniform sampler2D _normal_03;
		uniform float4 _normal_03_ST;
		uniform float _normal_03_scale;
		uniform sampler2D _normal_04;
		uniform float4 _normal_04_ST;
		uniform float _normal_04_scale;
		uniform sampler2D _normal_05_2u3u;
		uniform float4 _normal_05_2u3u_ST;
		uniform float _normal_05_scale;
		uniform sampler2D _MainTex_04_B;
		uniform float4 _MainTex_04_B_ST;
		uniform float4 _Color_04;
		uniform sampler2D _MainTex_03_G;
		uniform float4 _MainTex_03_G_ST;
		uniform float4 _Color_03;
		uniform sampler2D _MainTex_01_R;
		uniform float4 _MainTex_01_R_ST;
		uniform float4 _Color_01;
		uniform sampler2D _MainTex_02_back;
		uniform float4 _MainTex_02_back_ST;
		uniform float4 _Color_02;
		uniform sampler2D _MaTex_04;
		uniform float4 _MaTex_04_ST;
		uniform float _MaiTex_04_M_int;
		uniform sampler2D _MaTex_03;
		uniform float4 _MaTex_03_ST;
		uniform float _MaiTex_03_M_int;
		uniform sampler2D _MaiTex_01;
		uniform float4 _MaiTex_01_ST;
		uniform float _MaiTex_01_M_int;
		uniform sampler2D _MaTex_02;
		uniform float4 _MaTex_02_ST;
		uniform float _MaiTex_02_M_int;
		uniform float _MaiTex_04_S_int;
		uniform float _MaiTex_03_S_int;
		uniform float _MaiTex_01_S_int;
		uniform float _MaiTex_02_S_int;
		uniform float _MaiTex_04_Ao_int;
		uniform float _MaiTex_03_Ao_int;
		uniform float _MaiTex_01_Ao_int;
		uniform float _MaiTex_02_Ao_int;


		float3 ACESTonemap243( float3 linearcolor )
		{
			float a = 2.51f;
			float b = 0.03f;
			float c = 2.43f;
			float d = 0.59f;
			float e = 0.14f;
			return 
			saturate((linearcolor*(a*linearcolor+b))/(linearcolor*(c*linearcolor+d)+e));
		}


		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_normal_01 = i.uv_texcoord * _normal_01_ST.xy + _normal_01_ST.zw;
			float2 uv_MainTex_mask = i.uv_texcoord * _MainTex_mask_ST.xy + _MainTex_mask_ST.zw;
			float4 MainTexmask263 = tex2D( _MainTex_mask, uv_MainTex_mask );
			float3 temp_cast_0 = ((MainTexmask263).r).xxx;
			float grayscale203 = Luminance(temp_cast_0);
			float2 uv_normal_02 = i.uv_texcoord * _normal_02_ST.xy + _normal_02_ST.zw;
			float3 temp_cast_1 = ((MainTexmask263).g).xxx;
			float grayscale207 = Luminance(temp_cast_1);
			float2 uv_normal_03 = i.uv_texcoord * _normal_03_ST.xy + _normal_03_ST.zw;
			float3 temp_cast_2 = ((MainTexmask263).g).xxx;
			float grayscale214 = Luminance(temp_cast_2);
			float3 temp_cast_3 = ((MainTexmask263).b).xxx;
			float grayscale213 = Luminance(temp_cast_3);
			float temp_output_217_0 = ( grayscale214 * grayscale213 );
			float2 uv_normal_04 = i.uv_texcoord * _normal_04_ST.xy + _normal_04_ST.zw;
			float3 temp_output_403_0 = ( ( ( ( ( ( UnpackScaleNormal( tex2D( _normal_01, uv_normal_01 ), _normal_01_scale ) * grayscale203 ) + ( UnpackScaleNormal( tex2D( _normal_02, uv_normal_02 ), _normal_02_scale ) * ( 1.0 - grayscale203 ) ) ) * ( 1.0 - grayscale207 ) ) + ( UnpackScaleNormal( tex2D( _normal_03, uv_normal_03 ), _normal_03_scale ) * grayscale207 ) ) * ( 1.0 - temp_output_217_0 ) ) + ( UnpackScaleNormal( tex2D( _normal_04, uv_normal_04 ), _normal_04_scale ) * temp_output_217_0 ) );
			float2 uv2_normal_05_2u3u = i.uv2_texcoord2 * _normal_05_2u3u_ST.xy + _normal_05_2u3u_ST.zw;
			float2 uv3_normal_05_2u3u = i.uv3_texcoord3 * _normal_05_2u3u_ST.xy + _normal_05_2u3u_ST.zw;
			#ifdef _NORMAL_05_2U3U_ON
				float2 staticSwitch546 = uv3_normal_05_2u3u;
			#else
				float2 staticSwitch546 = uv2_normal_05_2u3u;
			#endif
			float3 tex2DNode154 = UnpackScaleNormal( tex2D( _normal_05_2u3u, staticSwitch546 ), _normal_05_scale );
			float2 appendResult408 = (float2(tex2DNode154.xy));
			float3 appendResult419 = (float3(( (temp_output_403_0).xy + appendResult408 ) , ( (temp_output_403_0).z + (tex2DNode154).z )));
			float3 normal402 = appendResult419;
			o.Normal = normal402;
			float2 uv_MainTex_04_B = i.uv_texcoord * _MainTex_04_B_ST.xy + _MainTex_04_B_ST.zw;
			float4 MainTex_mask23_g37 = tex2D( _MainTex_mask, uv_MainTex_mask );
			float3 temp_cast_5 = ((MainTex_mask23_g37).b).xxx;
			float grayscale16_g37 = Luminance(temp_cast_5);
			float2 uv_MainTex_03_G = i.uv_texcoord * _MainTex_03_G_ST.xy + _MainTex_03_G_ST.zw;
			float3 temp_cast_6 = ((MainTex_mask23_g37).g).xxx;
			float grayscale7_g37 = Luminance(temp_cast_6);
			float2 uv_MainTex_01_R = i.uv_texcoord * _MainTex_01_R_ST.xy + _MainTex_01_R_ST.zw;
			float3 temp_cast_7 = ((MainTex_mask23_g37).r).xxx;
			float grayscale3_g37 = Luminance(temp_cast_7);
			float2 uv_MainTex_02_back = i.uv_texcoord * _MainTex_02_back_ST.xy + _MainTex_02_back_ST.zw;
			float4 temp_output_296_0 = ( ( ( tex2D( _MainTex_04_B, uv_MainTex_04_B ) * _Color_04 ) * grayscale16_g37 ) + ( ( ( ( tex2D( _MainTex_03_G, uv_MainTex_03_G ) * _Color_03 ) * grayscale7_g37 ) + ( ( ( ( tex2D( _MainTex_01_R, uv_MainTex_01_R ) * _Color_01 ) * grayscale3_g37 ) + ( ( tex2D( _MainTex_02_back, uv_MainTex_02_back ) * _Color_02 ) * ( 1.0 - grayscale3_g37 ) ) ) * ( 1.0 - grayscale7_g37 ) ) ) * ( 1.0 - grayscale16_g37 ) ) );
			float3 linearcolor243 = ( temp_output_296_0 * temp_output_296_0 ).rgb;
			float3 localACESTonemap243 = ACESTonemap243( linearcolor243 );
			float3 color535 = sqrt( localACESTonemap243 );
			o.Albedo = color535;
			float2 uv_MaTex_04 = i.uv_texcoord * _MaTex_04_ST.xy + _MaTex_04_ST.zw;
			float3 temp_cast_9 = ((MainTexmask263).b).xxx;
			float grayscale345 = Luminance(temp_cast_9);
			float2 uv_MaTex_03 = i.uv_texcoord * _MaTex_03_ST.xy + _MaTex_03_ST.zw;
			float3 temp_cast_10 = ((MainTexmask263).g).xxx;
			float grayscale357 = Luminance(temp_cast_10);
			float2 uv_MaiTex_01 = i.uv_texcoord * _MaiTex_01_ST.xy + _MaiTex_01_ST.zw;
			float3 temp_cast_11 = ((MainTexmask263).r).xxx;
			float grayscale352 = Luminance(temp_cast_11);
			float2 uv_MaTex_02 = i.uv_texcoord * _MaTex_02_ST.xy + _MaTex_02_ST.zw;
			float metallic370 = ( ( ( tex2D( _MaTex_04, uv_MaTex_04 ).r * _MaiTex_04_M_int ) * grayscale345 ) + ( ( ( ( tex2D( _MaTex_03, uv_MaTex_03 ).r * _MaiTex_03_M_int ) * grayscale357 ) + ( ( ( ( ( tex2D( _MaiTex_01, uv_MaiTex_01 ).r * _MaiTex_01_M_int ) * grayscale352 ) * grayscale352 ) + ( ( tex2D( _MaTex_02, uv_MaTex_02 ).r * _MaiTex_02_M_int ) * ( 1.0 - grayscale352 ) ) ) * ( 1.0 - grayscale357 ) ) ) * ( 1.0 - grayscale345 ) ) );
			o.Metallic = metallic370;
			float3 temp_cast_12 = ((MainTexmask263).b).xxx;
			float grayscale480 = Luminance(temp_cast_12);
			float3 temp_cast_13 = ((MainTexmask263).g).xxx;
			float grayscale471 = Luminance(temp_cast_13);
			float3 temp_cast_14 = ((MainTexmask263).r).xxx;
			float grayscale461 = Luminance(temp_cast_14);
			float smothness491 = ( ( ( tex2D( _MaTex_04, uv_MaTex_04 ).g * _MaiTex_04_S_int ) * grayscale480 ) + ( ( ( ( tex2D( _MaTex_03, uv_MaTex_03 ).g * _MaiTex_03_S_int ) * grayscale471 ) + ( ( ( ( ( tex2D( _MaiTex_01, uv_MaiTex_01 ).g * _MaiTex_01_S_int ) * grayscale461 ) * grayscale461 ) + ( ( tex2D( _MaTex_02, uv_MaTex_02 ).g * _MaiTex_02_S_int ) * ( 1.0 - grayscale461 ) ) ) * ( 1.0 - grayscale471 ) ) ) * ( 1.0 - grayscale480 ) ) );
			o.Smoothness = smothness491;
			float3 temp_cast_15 = ((MainTexmask263).b).xxx;
			float grayscale522 = Luminance(temp_cast_15);
			float3 temp_cast_16 = ((MainTexmask263).g).xxx;
			float grayscale518 = Luminance(temp_cast_16);
			float3 temp_cast_17 = ((MainTexmask263).r).xxx;
			float grayscale524 = Luminance(temp_cast_17);
			float Ao529 = ( ( ( tex2D( _MaTex_04, uv_MaTex_04 ).b * _MaiTex_04_Ao_int ) * grayscale522 ) + ( ( ( ( tex2D( _MaTex_03, uv_MaTex_03 ).b * _MaiTex_03_Ao_int ) * grayscale518 ) + ( ( ( ( ( tex2D( _MaiTex_01, uv_MaiTex_01 ).b * _MaiTex_01_Ao_int ) * grayscale524 ) * grayscale524 ) + ( ( tex2D( _MaTex_02, uv_MaTex_02 ).b * _MaiTex_02_Ao_int ) * ( 1.0 - grayscale524 ) ) ) * ( 1.0 - grayscale518 ) ) ) * ( 1.0 - grayscale522 ) ) );
			#ifdef _AO_ONOFF_ON
				float staticSwitch300 = Ao529;
			#else
				float staticSwitch300 = 1.0;
			#endif
			o.Occlusion = staticSwitch300;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18400
2560;258;1440;1518;238.7219;-9077.961;1;True;True
Node;AmplifyShaderEditor.CommentaryNode;536;-679.7444,5069.705;Inherit;False;2489.005;1827.543;Color;16;251;264;263;252;256;257;290;293;253;292;294;296;245;243;244;535;Color;1,1,1,1;0;0
Node;AmplifyShaderEditor.TexturePropertyNode;251;-138.2165,5500.02;Inherit;True;Property;_MainTex_mask;MainTex_mask;4;0;Create;True;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SamplerNode;264;-83.81947,5119.705;Inherit;True;Property;_TextureSample0;Texture Sample 0;20;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;534;-684.5398,7018.795;Inherit;False;3373.249;3244.94;normal;46;402;198;200;151;150;199;203;202;205;148;143;204;208;209;211;226;207;212;213;214;225;215;400;218;217;219;229;228;221;401;223;222;149;154;403;423;417;422;408;418;421;419;156;152;546;547;normal;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;263;306.6277,5296.8;Inherit;False;MainTexmask;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;198;66.39826,7293.12;Inherit;True;263;MainTexmask;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;528;-3806.365,9403.741;Inherit;False;3036.087;2050.319;Ao;35;493;494;496;497;498;499;500;501;502;503;504;505;506;509;510;511;512;513;514;515;516;517;518;519;520;521;522;523;524;495;525;526;527;507;529;Ao;1,1,1,1;0;0
Node;AmplifyShaderEditor.SwizzleNode;200;283.0412,7305.662;Inherit;True;FLOAT;0;1;2;3;1;0;COLOR;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;368;-3722.083,5140.583;Inherit;False;2816.308;2065.993;metallic;35;337;338;339;340;342;343;345;347;348;351;352;355;357;362;363;364;366;367;349;350;344;346;370;442;443;444;445;447;450;448;451;452;453;455;446;metallic;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;199;274.4841,7860.346;Inherit;True;263;MainTexmask;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;492;-3806.024,7275.814;Inherit;False;3036.086;2051.318;smothness;35;456;458;459;462;463;464;465;466;468;469;470;471;472;473;475;476;477;478;479;480;481;482;484;485;486;487;488;489;467;461;457;460;474;483;491;smothness;1,1,1,1;0;0
Node;AmplifyShaderEditor.TFHCGrayscale;203;494.9221,7196.526;Inherit;False;0;1;0;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;151;-488.8606,7664.667;Inherit;False;Property;_normal_02_scale;normal_02_scale;14;0;Create;True;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;150;-634.5399,7398.521;Inherit;False;Property;_normal_01_scale;normal_01_scale;7;0;Create;True;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;509;-3611.248,9870.861;Inherit;True;263;MainTexmask;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;349;-3534.233,5613.515;Inherit;True;263;MainTexmask;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;148;-286.6744,7633.659;Inherit;True;Property;_normal_02;normal_02;13;0;Create;True;0;0;False;0;False;-1;None;None;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;456;-3610.907,7742.943;Inherit;True;263;MainTexmask;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SwizzleNode;511;-3394.605,9883.411;Inherit;True;FLOAT;0;1;2;3;1;0;COLOR;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;510;-3756.365,9448.251;Inherit;True;Property;_TextureSample10;Texture Sample 10;6;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Instance;353;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;525;-3437.667,9644.021;Inherit;False;Property;_MaiTex_01_Ao_int;MaiTex_01_Ao_int;11;0;Create;True;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;205;737.3534,7305.067;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SwizzleNode;202;715.4105,7923.173;Inherit;True;FLOAT;1;1;2;3;1;0;COLOR;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;204;262.0192,8341.776;Inherit;True;263;MainTexmask;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;143;-408.7068,7254.959;Inherit;True;Property;_normal_01;normal_01;6;0;Create;True;0;0;False;0;False;-1;None;None;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;442;-3679.349,5196.383;Inherit;True;Property;_TextureSample1;Texture Sample 1;6;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Instance;353;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;526;-3433.317,10375.04;Inherit;False;Property;_MaiTex_02_Ao_int;MaiTex_02_Ao_int;18;0;Create;True;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SwizzleNode;212;586.9451,8416.604;Inherit;True;FLOAT;2;1;2;3;1;0;COLOR;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SwizzleNode;209;590.6624,8184.855;Inherit;True;FLOAT;1;1;2;3;1;0;COLOR;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;514;-3490.78,10144.56;Inherit;True;Property;_TextureSample11;Texture Sample 11;16;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Instance;360;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;211;940.7364,7068.795;Inherit;True;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SwizzleNode;337;-3317.59,5626.057;Inherit;True;FLOAT;0;1;2;3;1;0;COLOR;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;447;-3285.135,5476.557;Inherit;True;Property;_MaiTex_01_M_int;MaiTex_01_M_int;9;0;Create;True;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;208;960.5116,7336.641;Inherit;True;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SwizzleNode;459;-3394.264,7755.494;Inherit;True;FLOAT;0;1;2;3;1;0;COLOR;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;226;-527.4338,8004.713;Inherit;False;Property;_normal_03_scale;normal_03_scale;21;0;Create;True;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCGrayscale;207;997.8107,7954.174;Inherit;False;0;1;0;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;457;-3361.81,7605.994;Inherit;False;Property;_MaiTex_01_S_int;MaiTex_01_S_int;10;0;Create;True;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCGrayscale;524;-3199.879,9896.031;Inherit;True;0;1;0;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;512;-3136.185,9599.841;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;458;-3756.024,7325.814;Inherit;True;Property;_TextureSample5;Texture Sample 5;6;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Instance;353;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;513;-3749.098,10500.07;Inherit;True;263;MainTexmask;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;400;1371.715,7374.936;Inherit;True;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TFHCGrayscale;352;-3125.71,5632.918;Inherit;False;0;1;0;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;462;-3135.844,7471.924;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;463;-3748.758,8372.151;Inherit;True;263;MainTexmask;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;460;-3072.718,8154.763;Inherit;False;Property;_MaiTex_02_S_int;MaiTex_02_S_int;17;0;Create;True;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SwizzleNode;515;-3484.791,10509.08;Inherit;True;FLOAT;1;1;2;3;1;0;COLOR;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;464;-3366.064,8038.083;Inherit;True;Property;_TextureSample6;Texture Sample 6;16;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Instance;360;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;446;-3059.17,5342.492;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;523;-2995.954,10025.71;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;450;-2996.043,6025.328;Inherit;False;Property;_MaiTex_02_M_int;MaiTex_02_M_int;16;0;Create;True;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;516;-2848.502,9643.571;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCGrayscale;461;-3118.385,7753.354;Inherit;False;0;1;0;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;225;-228.6578,7936.713;Inherit;True;Property;_normal_03;normal_03;20;0;Create;True;0;0;False;0;False;-1;None;None;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TFHCGrayscale;213;794.9443,8363.605;Inherit;False;0;1;0;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;517;-2911.208,10174.22;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;443;-3289.389,5908.654;Inherit;True;Property;_TextureSample2;Texture Sample 2;16;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Instance;360;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;350;-3672.083,6242.717;Inherit;True;263;MainTexmask;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;215;1408.951,7910.607;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCGrayscale;214;844.1046,8209.156;Inherit;False;0;1;0;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCGrayscale;518;-3216.173,10521.89;Inherit;False;0;1;0;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;355;-2906.947,5675.376;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;218;1732.043,7793.909;Inherit;True;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;520;-3435.627,11030.75;Inherit;True;263;MainTexmask;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;467;-2967.621,7841.804;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;493;-2715.123,10039.19;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;527;-2983.973,10924.61;Inherit;False;Property;_MaiTex_03_Ao_int;MaiTex_03_Ao_int;25;0;Create;True;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;466;-2848.162,7515.653;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;468;-2910.868,8046.303;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;519;-3092.514,10712.29;Inherit;True;Property;_TextureSample12;Texture Sample 12;23;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Instance;356;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;219;1372.097,7596.215;Inherit;True;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;229;1014.906,8592.547;Inherit;False;Property;_normal_04_scale;normal_04_scale;28;0;Create;True;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;217;1197.434,8181.429;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SwizzleNode;465;-3484.45,8381.162;Inherit;True;FLOAT;1;1;2;3;1;0;COLOR;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SwizzleNode;367;-3407.776,6251.731;Inherit;True;FLOAT;1;1;2;3;1;0;COLOR;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;494;-2636.824,9715.371;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;362;-2771.487,5386.22;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;448;-2834.193,5916.873;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;340;-2679.896,5505.188;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;452;-2906.958,6667.265;Inherit;False;Property;_MaiTex_03_M_int;MaiTex_03_M_int;23;0;Create;True;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;401;2071.715,7951.937;Inherit;True;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;495;-2726.802,10680.63;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;347;-2660.12,5773.035;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;344;-3358.612,6773.39;Inherit;True;263;MainTexmask;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;470;-2756.571,7634.624;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;221;1579.814,8108.639;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;444;-3015.499,6454.936;Inherit;True;Property;_TextureSample3;Texture Sample 3;23;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Instance;356;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;497;-2288.532,10535.43;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCGrayscale;357;-3139.158,6264.544;Inherit;False;0;1;0;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;496;-2490.739,9979.381;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCGrayscale;471;-3215.832,8393.972;Inherit;False;0;1;0;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;149;92.2464,9534.672;Inherit;False;1;154;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;228;1280.907,8512.545;Inherit;True;Property;_normal_04;normal_04;27;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;474;-2983.633,8796.691;Inherit;False;Property;_MaiTex_03_S_int;MaiTex_03_S_int;24;0;Create;True;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;547;84.60107,9695.737;Inherit;False;2;154;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;469;-2736.795,7902.464;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;472;-3092.174,8584.372;Inherit;True;Property;_TextureSample7;Texture Sample 7;23;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Instance;356;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;473;-3435.286,8902.822;Inherit;True;263;MainTexmask;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SwizzleNode;521;-3140.145,11058.39;Inherit;True;FLOAT;2;1;2;3;1;0;COLOR;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;293;-616.317,6028.678;Inherit;False;Property;_Color_03;Color _03;2;0;Create;True;0;0;False;0;False;0,0,0,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;366;-2211.517,6278.078;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCGrayscale;522;-2902.702,11057.35;Inherit;False;0;1;0;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;252;-96.43751,5738.195;Inherit;True;Property;_MainTex_01_R;MainTex_01_R;5;0;Create;True;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;475;-2737.25,8529.332;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SwizzleNode;476;-3139.805,8930.472;Inherit;True;FLOAT;2;1;2;3;1;0;COLOR;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;253;-191.6004,5956.674;Inherit;True;Property;_MainTex_02_back;MainTex_02_back;12;0;Create;True;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SimpleAddOpNode;339;-2413.724,5722.031;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;294;-408.6067,6690.248;Inherit;False;Property;_Color_04;Color _04;3;0;Create;True;0;0;False;0;False;0,0,0,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;222;1983.697,8468.996;Inherit;True;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TexturePropertyNode;256;-68.76954,6146.342;Inherit;True;Property;_MainTex_03_G;MainTex_03_G;19;0;Create;True;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.TexturePropertyNode;257;-89.16944,6375.217;Inherit;True;Property;_MainTex_04_B;MainTex_04_B;26;0;Create;True;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.StaticSwitch;546;371.6011,9653.737;Inherit;False;Property;_normal_05_2u3u;normal_05_2u/3u;39;0;Create;True;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;9;1;FLOAT2;0,0;False;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT2;0,0;False;6;FLOAT2;0,0;False;7;FLOAT2;0,0;False;8;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;477;-2490.399,7851.464;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;223;2402.63,8225.496;Inherit;True;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;156;299.2692,9797.035;Inherit;False;Property;_normal_05_scale;normal_05_scale;34;0;Create;True;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;499;-1965.606,10487.65;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;290;-613.8241,5815.83;Inherit;False;Property;_Color_01;Color _01;0;0;Create;True;0;0;False;0;False;0,0,0,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;500;-2831.148,11186.41;Inherit;True;Property;_TextureSample9;Texture Sample 9;27;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Instance;341;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;498;-2325.551,10289.96;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SwizzleNode;364;-3063.131,6801.038;Inherit;True;FLOAT;2;1;2;3;1;0;COLOR;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;451;-2660.575,6399.904;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;507;-2525.214,11339.05;Inherit;False;Property;_MaiTex_04_Ao_int;MaiTex_04_Ao_int;32;0;Create;True;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;292;-629.7445,6312.256;Inherit;False;Property;_Color_02;Color _02;1;0;Create;True;0;0;False;0;False;0,0,0,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;478;-2288.192,8407.512;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;482;-2830.808,9058.492;Inherit;True;Property;_TextureSample8;Texture Sample 8;27;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Instance;341;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;154;621.0085,9697.085;Inherit;True;Property;_normal_05_2u3u;normal_05_2u/3u;33;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;503;-2117.835,10802.38;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;455;-2449.199,7082.699;Inherit;False;Property;_MaiTex_04_M_int;MaiTex_04_M_int;30;0;Create;True;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;403;914.6505,9078.492;Inherit;True;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TFHCGrayscale;345;-2825.687,6799.997;Inherit;False;0;1;0;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;351;-2248.536,6032.608;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;296;275.5306,5854.851;Inherit;False;ground_block;-1;;37;6a704c28ac0167e499c571dbb6f3b859;0;9;22;SAMPLER2D;0;False;24;SAMPLER2D;0;False;30;SAMPLER2D;0;False;35;SAMPLER2D;0;False;39;SAMPLER2D;0;False;43;COLOR;0,0,0,0;False;42;COLOR;0,0,0,0;False;41;COLOR;0,0,0,0;False;44;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;502;-2430.315,11178.65;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;481;-1965.266,8359.731;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCGrayscale;480;-2902.362,8929.432;Inherit;False;0;1;0;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;479;-2325.211,8162.043;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;445;-2754.133,6929.06;Inherit;True;Property;_TextureSample4;Texture Sample 4;27;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Instance;341;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;483;-2525.874,9212.132;Inherit;False;Property;_MaiTex_04_S_int;MaiTex_04_S_int;31;0;Create;True;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;501;-1722.367,10410.68;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;342;-1888.591,6230.302;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;486;-2117.495,8674.462;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;453;-2353.301,6921.3;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;338;-1645.352,6153.327;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;343;-2040.82,6545.032;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;484;-1722.027,8282.762;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SwizzleNode;417;1137.782,9362.197;Inherit;True;FLOAT2;0;1;2;3;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;485;-2429.975,9050.731;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;505;-1513.468,10707.5;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;408;1178.7,9786.715;Inherit;True;FLOAT2;4;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SwizzleNode;423;1424.179,10021.59;Inherit;False;FLOAT;2;1;2;3;1;0;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;504;-2146.795,11051.91;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;245;764.2524,6052.454;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SwizzleNode;422;1432.093,9925.314;Inherit;False;FLOAT;2;1;2;3;1;0;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;487;-2146.455,8923.992;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;418;1513.916,9488.777;Inherit;True;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;348;-2069.78,6794.56;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CustomExpressionNode;243;1220.283,6195.642;Inherit;False;float a = 2.51f@$float b = 0.03f@$float c = 2.43f@$float d = 0.59f@$float e = 0.14f@$return $saturate((linearcolor*(a*linearcolor+b))/(linearcolor*(c*linearcolor+d)+e))@;3;False;1;True;linearcolor;FLOAT3;0,0,0;In;;Inherit;False;ACESTonemap;True;False;0;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;421;1655.065,10010.73;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;363;-1436.453,6450.14;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;488;-1513.128,8579.572;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;506;-1495.184,11040.08;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SqrtOpNode;244;1438.467,6203.903;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;489;-1494.844,8912.162;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;529;-1059.11,11007.13;Inherit;False;Ao;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;346;-1418.169,6782.729;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;419;1806.093,9457.558;Inherit;True;FLOAT3;4;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;491;-997.938,8990.762;Inherit;False;smothness;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;532;5161.757,6842.336;Inherit;True;529;Ao;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;402;2460.71,9637.692;Inherit;True;normal;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;535;1581.26,6268.75;Inherit;False;color;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;370;-1093.238,6853.541;Inherit;False;metallic;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;302;5233.835,6675.309;Inherit;False;Constant;_AO;AO;29;0;Create;True;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;541;4207.817,5497.994;Inherit;True;Property;_MaiTex_01;MaiTex_01;8;0;Create;True;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;436;4602.358,6477.942;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;360;3901.932,5752.332;Inherit;True;Property;_MaTex_021;MaTex_021;16;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;542;6005.018,6105.322;Inherit;False;535;color;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;353;4494.061,5440.404;Inherit;True;Property;_MaiTex_011;MaiTex_011;6;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;543;5148.875,6044.756;Inherit;True;Property;_emtex_mask;emtex_mask;38;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.BlendNormalsNode;152;1638.099,9050.195;Inherit;True;0;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;341;3910.906,5528.44;Inherit;True;Property;_MaTex_041;MaTex_041;27;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TexturePropertyNode;538;3644.298,5514.197;Inherit;True;Property;_MaTex_04;MaTex_04;29;0;Create;True;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.GetLocalVarNode;531;5958.568,6481.399;Inherit;False;491;smothness;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;439;4026.541,6177.887;Inherit;True;Property;_emtex;emtex;37;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StaticSwitch;300;5647.821,6814.947;Inherit;False;Property;_AO_onoff;AO_on/off;35;0;Create;True;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;409;5940.025,6232.364;Inherit;False;402;normal;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;433;5582.515,6385.474;Inherit;True;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TFHCGrayscale;545;5238.053,6223.381;Inherit;False;0;1;0;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SwizzleNode;438;4982.017,6181.624;Inherit;True;FLOAT;3;1;2;3;1;0;COLOR;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;435;4140.848,6453.562;Inherit;False;Property;_emission_Color;emission_Color;36;0;Create;True;0;0;False;0;False;0,0,0,0;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;437;4581.505,6121.216;Inherit;False;263;MainTexmask;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;356;4496.722,5760.574;Inherit;True;Property;_MaTex_031;MaTex_031;23;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;530;5948.938,6338.192;Inherit;False;370;metallic;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;539;4230.306,5749.824;Inherit;True;Property;_MaTex_03;MaTex_03;22;0;Create;True;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.TexturePropertyNode;540;3670.406,5758.858;Inherit;True;Property;_MaTex_02;MaTex_02;15;0;Create;True;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;6291.969,6268.387;Float;False;True;-1;3;ASEMaterialInspector;0;0;Standard;bud/ground_block;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;264;0;251;0
WireConnection;263;0;264;0
WireConnection;200;0;198;0
WireConnection;203;0;200;0
WireConnection;148;5;151;0
WireConnection;511;0;509;0
WireConnection;205;0;203;0
WireConnection;202;0;199;0
WireConnection;143;5;150;0
WireConnection;212;0;204;0
WireConnection;209;0;204;0
WireConnection;211;0;143;0
WireConnection;211;1;203;0
WireConnection;337;0;349;0
WireConnection;208;0;148;0
WireConnection;208;1;205;0
WireConnection;459;0;456;0
WireConnection;207;0;202;0
WireConnection;524;0;511;0
WireConnection;512;0;510;3
WireConnection;512;1;525;0
WireConnection;400;0;211;0
WireConnection;400;1;208;0
WireConnection;352;0;337;0
WireConnection;462;0;458;2
WireConnection;462;1;457;0
WireConnection;515;0;513;0
WireConnection;446;0;442;1
WireConnection;446;1;447;0
WireConnection;523;0;524;0
WireConnection;516;0;512;0
WireConnection;516;1;524;0
WireConnection;461;0;459;0
WireConnection;225;5;226;0
WireConnection;213;0;212;0
WireConnection;517;0;514;3
WireConnection;517;1;526;0
WireConnection;215;0;207;0
WireConnection;214;0;209;0
WireConnection;518;0;515;0
WireConnection;355;0;352;0
WireConnection;218;0;400;0
WireConnection;218;1;215;0
WireConnection;467;0;461;0
WireConnection;493;0;517;0
WireConnection;493;1;523;0
WireConnection;466;0;462;0
WireConnection;466;1;461;0
WireConnection;468;0;464;2
WireConnection;468;1;460;0
WireConnection;219;0;225;0
WireConnection;219;1;207;0
WireConnection;217;0;214;0
WireConnection;217;1;213;0
WireConnection;465;0;463;0
WireConnection;367;0;350;0
WireConnection;494;0;516;0
WireConnection;494;1;524;0
WireConnection;362;0;446;0
WireConnection;362;1;352;0
WireConnection;448;0;443;1
WireConnection;448;1;450;0
WireConnection;340;0;362;0
WireConnection;340;1;352;0
WireConnection;401;0;218;0
WireConnection;401;1;219;0
WireConnection;495;0;519;3
WireConnection;495;1;527;0
WireConnection;347;0;448;0
WireConnection;347;1;355;0
WireConnection;470;0;466;0
WireConnection;470;1;461;0
WireConnection;221;0;217;0
WireConnection;497;0;518;0
WireConnection;357;0;367;0
WireConnection;496;0;494;0
WireConnection;496;1;493;0
WireConnection;471;0;465;0
WireConnection;228;5;229;0
WireConnection;469;0;468;0
WireConnection;469;1;467;0
WireConnection;521;0;520;0
WireConnection;366;0;357;0
WireConnection;522;0;521;0
WireConnection;475;0;472;2
WireConnection;475;1;474;0
WireConnection;476;0;473;0
WireConnection;339;0;340;0
WireConnection;339;1;347;0
WireConnection;222;0;228;0
WireConnection;222;1;217;0
WireConnection;546;1;149;0
WireConnection;546;0;547;0
WireConnection;477;0;470;0
WireConnection;477;1;469;0
WireConnection;223;0;401;0
WireConnection;223;1;221;0
WireConnection;499;0;496;0
WireConnection;499;1;497;0
WireConnection;498;0;495;0
WireConnection;498;1;518;0
WireConnection;364;0;344;0
WireConnection;451;0;444;1
WireConnection;451;1;452;0
WireConnection;478;0;471;0
WireConnection;154;1;546;0
WireConnection;154;5;156;0
WireConnection;503;0;522;0
WireConnection;403;0;223;0
WireConnection;403;1;222;0
WireConnection;345;0;364;0
WireConnection;351;0;451;0
WireConnection;351;1;357;0
WireConnection;296;22;251;0
WireConnection;296;24;252;0
WireConnection;296;30;253;0
WireConnection;296;35;256;0
WireConnection;296;39;257;0
WireConnection;296;43;290;0
WireConnection;296;42;292;0
WireConnection;296;41;293;0
WireConnection;296;44;294;0
WireConnection;502;0;500;3
WireConnection;502;1;507;0
WireConnection;481;0;477;0
WireConnection;481;1;478;0
WireConnection;480;0;476;0
WireConnection;479;0;475;0
WireConnection;479;1;471;0
WireConnection;501;0;498;0
WireConnection;501;1;499;0
WireConnection;342;0;339;0
WireConnection;342;1;366;0
WireConnection;486;0;480;0
WireConnection;453;0;445;1
WireConnection;453;1;455;0
WireConnection;338;0;351;0
WireConnection;338;1;342;0
WireConnection;343;0;345;0
WireConnection;484;0;479;0
WireConnection;484;1;481;0
WireConnection;417;0;403;0
WireConnection;485;0;482;2
WireConnection;485;1;483;0
WireConnection;505;0;501;0
WireConnection;505;1;503;0
WireConnection;408;0;154;0
WireConnection;423;0;154;0
WireConnection;504;0;502;0
WireConnection;504;1;522;0
WireConnection;245;0;296;0
WireConnection;245;1;296;0
WireConnection;422;0;403;0
WireConnection;487;0;485;0
WireConnection;487;1;480;0
WireConnection;418;0;417;0
WireConnection;418;1;408;0
WireConnection;348;0;453;0
WireConnection;348;1;345;0
WireConnection;243;0;245;0
WireConnection;421;0;422;0
WireConnection;421;1;423;0
WireConnection;363;0;338;0
WireConnection;363;1;343;0
WireConnection;488;0;484;0
WireConnection;488;1;486;0
WireConnection;506;0;504;0
WireConnection;506;1;505;0
WireConnection;244;0;243;0
WireConnection;489;0;487;0
WireConnection;489;1;488;0
WireConnection;529;0;506;0
WireConnection;346;0;348;0
WireConnection;346;1;363;0
WireConnection;419;0;418;0
WireConnection;419;2;421;0
WireConnection;491;0;489;0
WireConnection;402;0;419;0
WireConnection;535;0;244;0
WireConnection;370;0;346;0
WireConnection;436;0;439;0
WireConnection;436;1;435;0
WireConnection;360;0;540;0
WireConnection;353;0;541;0
WireConnection;152;0;403;0
WireConnection;152;1;408;0
WireConnection;341;0;538;0
WireConnection;300;1;302;0
WireConnection;300;0;532;0
WireConnection;433;0;545;0
WireConnection;433;1;436;0
WireConnection;545;0;438;0
WireConnection;438;0;437;0
WireConnection;356;0;539;0
WireConnection;0;0;542;0
WireConnection;0;1;409;0
WireConnection;0;3;530;0
WireConnection;0;4;531;0
WireConnection;0;5;300;0
ASEEND*/
//CHKSM=8FAD31EC5007EB77911047E6A7451179B376D861