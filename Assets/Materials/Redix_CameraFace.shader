
Shader "Redix/CameraFace"
{

Properties{
    [Space(10)]
    _Color("Color", Color) = (1,1,1,1)
    [Header(Textures)]
    [Space(10)]
    _MainTex("Albedo", 2D) = "white" {}
    _SpecularTex("Specular", 2D) = "black" {}

    [Header(Camera Facing)]
    [Space(10)]
    _CameraFace_minStrength("Camera Face Min Strength", Range(0,1)) = 0.4
    _CameraFace_maxStrength("Camera Face Max Strength", Range(0,1)) = 1.0
    _CameraFace_boostStrength("Camera Face Boost", Range(0,5)) = 1.0

    [Header(Diffuse and Specular)]
    [Space(10)]
    _DiffuseStrength("Diffuse Strength", Range(0,1)) = 0.2
    _SpecularSharp("Specular Total Sharpness", Range(1,20)) = 2
    _SpecularVisibility("Specular Total Visibility", Range(0,1)) = 1.0
    
    [Header(Fresnel)]
    [Space(10)]
    _TopFresnel_Color("Top Fresnel Color", Color) = (1,1,1,1)
    _BotFresnel_Color("Bottom Fresnel Color", Color) = (1,1,1,1)

    _Fresnel_Sharpness("Fresnel Sharpness", Range(0.01,20)) = 1.0

    _TopFresnel_Visibil("Bottom Fresnel Visibility", Range(0,1)) = 1.0
    _BotFresnel_Visibil("Bottom Fresnel Visibility", Range(0,1)) = 1.0

    [Header(Saturation)]
    [Space(10)]
    _Saturation("Saturation", Range(0,1)) = 1.0
}


SubShader
{
    Tags { "RenderType"="Opaque" }
    LOD 100

    Pass
    {
        Name "Base"
        Tags {"LightMode" = "ForwardBase"}

        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        #pragma multi_compile_fog// makes fog work.

        #include "UnityCG.cginc"
        #include "Lighting.cginc"
        #include "AutoLight.cginc"

        struct appdata
        {
            float4 vertex : POSITION;
            float3 normal : NORMAL;
            float2 uv : TEXCOORD0;
        };

        struct v2f
        {
            float4 vertex : SV_POSITION;
            float2 uv : TEXCOORD0;
            float3 worldNormal : TEXCOORD1;//in world-space
            float3 viewDir : TEXCOORD2; //in world-space
            float3 lightDir : TEXCOORD3;//in world-space
            UNITY_FOG_COORDS(4)
        };


        fixed4 _Color;

        sampler2D _MainTex;
        float4 _MainTex_ST;
        sampler2D _SpecularTex;

        float _CameraFace_Strength;

        float _CameraFace_minStrength;
        float _CameraFace_maxStrength;
        float _CameraFace_boostStrength;

        float _DiffuseStrength;
        float _SpecularSharp;
        float _SpecularVisibility;


        fixed4 _TopFresnel_Color;
        fixed4 _BotFresnel_Color;

        float _Fresnel_Sharpness;

        float _TopFresnel_Visibil;
        float _BotFresnel_Visibil;

        fixed _Saturation;


        v2f vert( appdata v ){
            v2f o;
            
            o.vertex =  UnityObjectToClipPos(v.vertex);
            o.uv = TRANSFORM_TEX(v.uv, _MainTex);
            
            o.worldNormal =  UnityObjectToWorldNormal( v.normal );

            //The direction from the current point to the camera
            o.viewDir = WorldSpaceViewDir( v.vertex );    
            o.lightDir = WorldSpaceLightDir( v.vertex );

            return o;
        }


        //tells how bright the fragment can be based on how well it faces towards the camera
        fixed get_cameraFaceAttenuation( v2f i, float normDotView01 ){
            float scaled_normDotView =  lerp( _CameraFace_minStrength,
                                              _CameraFace_maxStrength,  normDotView01 );
            return scaled_normDotView * _CameraFace_boostStrength;
        }


        fixed get_directionalLight_Diffuse( v2f i ){
            fixed normDotLight =  saturate( dot( i.lightDir,  i.worldNormal ) );
            fixed diffuse =  lerp(0, normDotLight, _DiffuseStrength);
            return diffuse;
        }


        fixed get_specularHighlight( v2f i,  float sharpness ){
            half3 halfView =  normalize ( i.lightDir + i.viewDir );
            fixed atten = saturate( dot(halfView, i.worldNormal) );
            return pow(atten, sharpness);
        }


        fixed4 get_fresnelColor( v2f i, float normDotView01 ){
            
            fixed botTopFresnel_coeff01 =  (dot(i.worldNormal, float3(0,1,0)) + 1)*0.5f;
            fixed botFresnelCoeff01 =  1.0f-botTopFresnel_coeff01;
                  botFresnelCoeff01 =  saturate((botFresnelCoeff01-0.5f)*2);
                 
            fixed topFresnelCoeff01 =  botTopFresnel_coeff01;
                  topFresnelCoeff01 =  saturate((topFresnelCoeff01-0.5f)*2);

            fixed4 fresnelColor =  lerp( fixed4(0,0,0,1), _BotFresnel_Color*_BotFresnel_Visibil,  botFresnelCoeff01)
                                  +lerp( fixed4(0,0,0,1), _TopFresnel_Color*_TopFresnel_Visibil,  topFresnelCoeff01);

            float fresnelCoeff =  pow( 1-normDotView01, _Fresnel_Sharpness );
            return fresnelCoeff*fresnelColor;
        }


        fixed4 frag (v2f i) : SV_Target{
            fixed4 albedo =  tex2D( _MainTex, i.uv );
            fixed4 specularTex =  tex2D( _SpecularTex, i.uv );

            i.worldNormal = normalize( i.worldNormal );//because might have been squashed during interpolation.
            i.viewDir =  normalize( i.viewDir );
            i.lightDir = normalize( i.lightDir );

            float normDotView01 =  saturate( dot(i.worldNormal, i.viewDir) );
            fixed cameraAtten =  get_cameraFaceAttenuation( i, normDotView01 );
            fixed diffuseAtten =  get_directionalLight_Diffuse( i );

            
            float specTexPlus1 = (1+specularTex);
            //make dark areas of  spec texture even more dark (decreses sharpenss of the shiny spec shiny):
            float specPower = specTexPlus1 * specTexPlus1 * specTexPlus1; 
            //boosts the overal sharpness of the shiny spec:
            specPower *= _SpecularSharp;

            fixed specHighlight =  get_specularHighlight( i, specPower );
            specHighlight *= specularTex; //keeps sharpness, but makes specular highlight more faint in darker areas.
            specHighlight *= _SpecularVisibility;//keeps sharpness, but makes spec highlight more faint everywhere.
            
            fixed4 fresnelColor =  get_fresnelColor( i, normDotView01 );


            fixed4 col =  albedo*_Color*cameraAtten
                        + albedo*_Color*diffuseAtten * _LightColor0
                        + specHighlight * _LightColor0
                        + fresnelColor;

            //make it all grayscale if needed:
            fixed grayscale =  Luminance(col);
            fixed4 saturatedColor =  lerp(fixed4(grayscale,grayscale,grayscale,1.0), col, _Saturation);
            
            // apply fog
            UNITY_APPLY_FOG(i.fogCoord, saturatedColor);
            return saturatedColor;
        }
        ENDCG
    }
}//end SubShader

Fallback "VertexLit"  //for casting the shadows onto other surfaces
}
