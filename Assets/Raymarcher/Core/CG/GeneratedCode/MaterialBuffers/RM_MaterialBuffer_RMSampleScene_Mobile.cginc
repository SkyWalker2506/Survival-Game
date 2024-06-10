// --------------------- Registered Global Keywords
#pragma shader_feature_fragment STANDARD_LIGHTING
#pragma shader_feature_fragment STANDARD_SPECULAR
#pragma shader_feature_fragment STANDARD_REFRACTION
#pragma shader_feature_fragment STANDARD_REFRACTION_MAGNIFY

// --------------------- Registered Global Uniforms

// --------------------- Registered Structured/Unpacked Data Containers
struct RMStandardMobileSLitData
{
    half4 colorOverride;
    half normalShift;
    half2 specularIntensAndGloss;
    half2 shadingCovAndSmh;
    half4 shadingTint;
};

    half4 colorOverride[1];
    half normalShift[1];
    half2 specularIntensAndGloss[1];
    half2 shadingCovAndSmh[1];
    half4 shadingTint[1];


// --------------------- Registered Texture Containers
Texture2DArray RMStandardMobileSLitDataTextures;
SamplerState sampler_RMStandardMobileSLitDataTextures;

// --------------------- Registered Method Contents

float3 CalculateNormals(in float3 rayPosition, in half normalShift)
{
	half2 offset = half2(normalShift, 0.0);

#ifdef RAYMARCHER_TYPE_QUALITY
	return normalize(SdfObjectBuffer(rayPosition)[0].x - float3(
		SdfObjectBuffer(rayPosition - offset.xyy)[0].x,
		SdfObjectBuffer(rayPosition - offset.yxy)[0].x,
		SdfObjectBuffer(rayPosition - offset.yyx)[0].x));
#else
	return normalize(SdfObjectBuffer(rayPosition).x - float3(
		SdfObjectBuffer(rayPosition - offset.xyy).x,
		SdfObjectBuffer(rayPosition - offset.yxy).x,
		SdfObjectBuffer(rayPosition - offset.yyx).x));
#endif
}

half3 CalculateTriplanarTexture(in half4 sdfRGBA, in float3 rayPosition, in half3 normal, 
in Texture2DArray tex2DArray, in SamplerState tex2DArraySampler, 
in half tiling, in half index, in half triBlend)
{
#ifdef STANDARD_TRIPLANAR_TEXTURE
	half3 texXY = RM_SAMPLE_ARRAY_SPECIFIC(tex2DArray, tex2DArraySampler, rayPosition.xy * tiling, index).rgb;
	half3 texXZ = RM_SAMPLE_ARRAY_SPECIFIC(tex2DArray, tex2DArraySampler, rayPosition.xz * tiling, index).rgb;
	half3 texYZ = RM_SAMPLE_ARRAY_SPECIFIC(tex2DArray, tex2DArraySampler, rayPosition.yz * tiling, index).rgb;

	half3 absNormal = abs(normal);
	absNormal *= pow(absNormal, max(EPSILONUP, triBlend));
	absNormal /= absNormal.x + absNormal.y + absNormal.z;
	return texXY * absNormal.z + texXZ * absNormal.y + texYZ * absNormal.x;
#else
	return sdfRGBA.rgb;
#endif
}

half CalculateBlinnPhongSpecular(in half specSize, in half specGlossiness, in half specIntensity, in float3 normals, in float3 rayDirection, in float3 directionToLight, in half attenuation = 1.0, in half overrideIntensity = 1.0)
{
	half specDot = max(0., dot(rayDirection, reflect(-directionToLight, normals)));
	return smoothstep(0, max(EPSILONUP, specGlossiness),
		pow(specDot, lerp(100. * lerp(5., 1., specSize), EPSILON, saturate(specSize))))
		* specIntensity * attenuation * overrideIntensity;
}

half CalculateLambertianModel(in half shadingCoverage, in half shadingSmoothness, in float3 normals, in float3 lightDirection)
{
	return max(0., smoothstep(shadingCoverage - shadingSmoothness, shadingCoverage + shadingSmoothness, dot(lightDirection, normals)));
}


half4 CalculateMobileSLitModelWrapper(in RMStandardMobileSLitData data, in Ray ray, half4 sdfRGBA, half3 normal)
{
#ifdef STANDARD_LIGHTING
	half3 lightingColor = 1;
	#ifdef RAYMARCHER_MAIN_LIGHT
		lightingColor = RaymarcherDirectionalLightColor.rgb * lerp(data.shadingTint.rgb, 1, CalculateLambertianModel(data.shadingCovAndSmh.x, data.shadingCovAndSmh.y, normal, -RaymarcherDirectionalLightDir))
			 * RaymarcherDirectionalLightColor.a;
		sdfRGBA.rgb *= lightingColor;
		// Specular
		#ifdef STANDARD_SPECULAR
			sdfRGBA.rgb += RaymarcherDirectionalLightColor.rgb * CalculateBlinnPhongSpecular(data.specularIntensAndGloss.y, data.specularIntensAndGloss.y, data.specularIntensAndGloss.x, normal, ray.nd, RaymarcherDirectionalLightDir.xyz, 1, RaymarcherDirectionalLightColor.a);
		#endif
	#endif
#endif

#ifdef STANDARD_REFRACTION
	#ifdef STANDARD_REFRACTION_MAGNIFY
		half3 sceneTex = CalculateSceneColor(ray.uv - normal.xy * 0.02).rgb;
	#else
		half3 sceneTex = CalculateSceneColor(ray.uv + normal.xy * 0.02).rgb;
	#endif
	sdfRGBA.rgb *= lerp(1., sceneTex, 0.8);
#endif

return sdfRGBA;
}

// --------------------- Registered Core Method Contents
half4 CalculateMobileSLitModel(in RMStandardMobileSLitData data, in Ray ray, half4 sdfRGBA)
{
	sdfRGBA.rgb = lerp(sdfRGBA.rgb, data.colorOverride.rgb, data.colorOverride.a);
	return CalculateMobileSLitModelWrapper(data, ray, sdfRGBA, CalculateNormals(ray.p, data.normalShift));
}

// ------------------- PER-OBJECT MATERIAL RENDERER ----------------------
half4 RM_PerObjRenderMaterials(in Ray ray, half4 sdfRGBA, in int sdfMaterialType, in int sdfMaterialInstance)
{
    half4 sdfRef = sdfRGBA;

    return sdfRGBA;
}
// -----------------------------------------------------------------------


// ------------------- PER-OBJECT POST MATERIAL RENDERER ----------------------
half4 RM_PerObjPostRenderMaterials(in Ray ray, half4 sdfRGBA, in int sdfMaterialType, in int sdfMaterialInstance)
{
    half4 sdfRef = sdfRGBA;

    return sdfRGBA;
}
// -----------------------------------------------------------------------

// ------------------- GLOBAL MATERIAL RENDERER ----------------------
half4 RM_GlobalRenderMaterials(in Ray ray, half4 sdfRGBA, in int temp0, in int temp1)
{
    RMStandardMobileSLitData RMStandardMobileSLitData_TempInstance = {colorOverride[0],normalShift[0],specularIntensAndGloss[0],shadingCovAndSmh[0],shadingTint[0]};
    sdfRGBA = CalculateMobileSLitModel(RMStandardMobileSLitData_TempInstance, ray, sdfRGBA);

    return sdfRGBA;
}
// -----------------------------------------------------------------------


// ------------------- GLOBAL POST MATERIAL RENDERER ----------------------
half4 RM_GlobalPostRenderMaterials(in Ray ray, half4 sdfRGBA, in int temp0, in int temp1)
{

    return sdfRGBA;
}
// -----------------------------------------------------------------------
