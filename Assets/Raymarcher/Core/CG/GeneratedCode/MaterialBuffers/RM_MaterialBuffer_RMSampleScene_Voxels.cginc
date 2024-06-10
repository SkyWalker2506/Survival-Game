// --------------------- Registered Global Keywords
#pragma shader_feature_fragment STANDARD_TRIPLANAR_TEXTURE
#pragma shader_feature_fragment STANDARD_FRESNEL
#pragma shader_feature_fragment STANDARD_LIGHTING
#pragma shader_feature_fragment STANDARD_ADDITIONAL_LIGHTS_LINEAR_ATTENUATION
#pragma shader_feature_fragment STANDARD_SAMPLE_TRANSLUCENCY
#pragma shader_feature_fragment STANDARD_SPECULAR
#pragma shader_feature_fragment STANDARD_SHADOWS
#pragma shader_feature_fragment STANDARD_SHADOWS_SOFT

// --------------------- Registered Global Uniforms
uniform half _StandardShadowQuality;
uniform half _StandardShadowSoftness;

// --------------------- Registered Structured/Unpacked Data Containers
struct RMStandardUnlitData
{
    half4 colorOverride;
    half colorBlend;
    half mainAlbedoOpacity;
    half mainAlbedoTiling;
    half mainAlbedoTriplanarBlend;
    half mainAlbedoIndex;
    half normalShift;
    half fresnelCoverage;
    half fresnelDensity;
    half4 fresnelColor;
};

struct RMStandardLitData
{
    RMStandardUnlitData unlitData;
    half specularIntensity;
    half specularSize;
    half specularGlossiness;
    half shadingCoverage;
    half shadingSmoothness;
    half3 shadingTint;
    half useShadows;
    half2 shadowDistanceMinMax;
    half shadowAmbience;
    half includeDirectionalLight;
    half includeAdditionalLights;
    half translucencyMinAbsorption;
    half translucencyMaxAbsorption;
};

struct RMStandardSlopeLitData
{
    RMStandardLitData litData;
    half slopeTextureIndex;
    half slopeCoverage;
    half slopeSmoothness;
    half slopeTextureBlend;
    half slopeTextureScatter;
    half slopeTextureEmission;
};

StructuredBuffer<RMStandardSlopeLitData> RMStandardSlopeLitDataInstance;

// --------------------- Registered Texture Containers
Texture2DArray RMStandardSlopeLitDataTextures;
SamplerState sampler_RMStandardSlopeLitDataTextures;

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
half4 CalculateUnlitModelWrapper(in RMStandardUnlitData data, in Ray ray, half4 sdfRGBA, in Texture2DArray texPack, in SamplerState texPackSampler, out half3 normal)
{
	sdfRGBA = lerp(sdfRGBA, data.colorOverride, data.colorBlend);
	normal = CalculateNormals(ray.p, data.normalShift);

	half3 tex = CalculateTriplanarTexture(
	sdfRGBA, ray.p, normal, 
	texPack, texPackSampler,
	data.mainAlbedoTiling, data.mainAlbedoIndex, data.mainAlbedoTriplanarBlend);

	sdfRGBA.rgb = lerp(sdfRGBA.rgb, tex, data.mainAlbedoOpacity);

#ifdef STANDARD_FRESNEL
	half fresnel = 1. - saturate((1. - data.fresnelCoverage) * pow(abs(1 - dot(ray.nd, normal)), data.fresnelDensity));
	sdfRGBA = lerp(sdfRGBA, data.fresnelColor, fresnel);
#endif

	return sdfRGBA;
}
float CalculateShadows(in RMStandardLitData data, in float3 rayOrigin, in float3 rayDirection, in float distanceToLight, in half shadowDistMinOverride = 0.0)
{
	float result = 1.0;
	float3 rayPos;

	for(float i = data.shadowDistanceMinMax.x + shadowDistMinOverride; i < data.shadowDistanceMinMax.y;)
	{
		rayPos = rayOrigin + (rayDirection * i);

#ifdef RAYMARCHER_TYPE_QUALITY
		float sdf = SdfObjectBuffer(rayPos)[0].x;
#else
		float sdf = SdfObjectBuffer(rayPos).x;
#endif

		if(sdf < EPSILON)
			return data.shadowAmbience;

#ifdef STANDARD_SHADOWS_SOFT
		result = min(result, _StandardShadowSoftness * (sdf / i));
#else
		result = max(result, 0.);
#endif

		i += clamp(sdf * _StandardShadowQuality, 0.01, 1.0); // shifting fixer

		if (i >= distanceToLight)
			break;
	}
	return result + data.shadowAmbience;
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


half4 CalculateFullLightingModel(half4 sdfRGBA, in RMStandardLitData data, in float3 rayHitPosition, in half3 rayNormalizedDirection, in half3 normals)
{
	half3 diffuse = half3(0, 0, 0);
	half3 specular = half3(0, 0, 0);

	// Main light Lambert
#ifdef RAYMARCHER_MAIN_LIGHT

	half3 mainLightDiffuse = (RaymarcherDirectionalLightColor.rgb * RaymarcherDirectionalLightColor.a)
		* lerp(data.shadingTint, 1, CalculateLambertianModel(data.shadingCoverage, data.shadingSmoothness, normals, -RaymarcherDirectionalLightDir));

	// Main light shadows
	half mainLightShadow = 1.0;
	#ifdef STANDARD_SHADOWS
		mainLightShadow = lerp(1,
			saturate(CalculateShadows(data, rayHitPosition + (normals * EPSILON), -RaymarcherDirectionalLightDir.xyz, 64)),
			data.useShadows);
		mainLightDiffuse *= mainLightShadow;
	#endif

	// Main light specular
	#ifdef STANDARD_SPECULAR
		specular = lerp(specular, RaymarcherDirectionalLightColor.rgb *
			CalculateBlinnPhongSpecular(
				data.specularSize,
				data.specularGlossiness,
				data.specularIntensity,
				normals,
				rayNormalizedDirection,
				RaymarcherDirectionalLightDir.xyz,
				1,
				RaymarcherDirectionalLightColor.a) * pow(mainLightShadow, 4),
			data.includeDirectionalLight);
	#endif

		diffuse = lerp(diffuse, mainLightDiffuse, data.includeDirectionalLight);

#endif


	// Add Lights Lambert
#if defined(RAYMARCHER_LIGHT_COUNT) && defined(RAYMARCHER_ADDITIONAL_LIGHTS)

	half3 addLightDiffuse = half3(0, 0, 0);
	half addLightShadow = 1.0;
	half3 addLightSpecular = half3(0, 0, 0);

	[unroll(RAYMARCHER_LIGHT_COUNT)] for (uint i = 0; i < RAYMARCHER_LIGHT_COUNT; i += 3)
	{
		// Add-Light data
		half4 p0 = RaymarcherAddLightsData[i];		// xyz = light position, w = light intensity
		half4 p1 = RaymarcherAddLightsData[i + 1];	// rgb = light color, w = light range
		half4 p2 = RaymarcherAddLightsData[i + 2];	// x = light shadow intensity, y = light shadow attenuation offset
		p0.w = abs(p0.w);
		p1.w = abs(p1.w);

		float3 dirToLight = p0.xyz - rayHitPosition;
		float distToLight = length(dirToLight);
		half lambertReflection = CalculateLambertianModel(data.shadingCoverage, data.shadingSmoothness, normalize(dirToLight), normals);

		// Attenuation & intensity
	#ifdef STANDARD_ADDITIONAL_LIGHTS_LINEAR_ATTENUATION
		float rangedAttenuation = saturate(1.0 - saturate(distToLight / p1.w)) * lambertReflection * p0.w;
	#else
		float rangedAttenuation = saturate(exp(-distToLight / (p1.w / 2.0))) * lambertReflection * p0.w;
	#endif

	#ifdef STANDARD_SAMPLE_TRANSLUCENCY
		#ifdef RAYMARCHER_TYPE_QUALITY
			float sdf = SdfObjectBuffer(p0.xyz + (normals * EPSILONUPUPTWO))[0].x;
		#else
			float sdf = SdfObjectBuffer(p0.xyz + (normals * EPSILONUPUPTWO)).x;
		#endif
			rangedAttenuation *= smoothstep(data.translucencyMinAbsorption, data.translucencyMaxAbsorption, sdf);
	#endif

		// Diffuse
		half3 currentColor = p1.rgb * lerp(data.shadingTint, 1, rangedAttenuation);
			
		// Shadows
	#ifdef STANDARD_SHADOWS
		float currentShadow = saturate(CalculateShadows(data, rayHitPosition + (normals * EPSILONUP), normalize(dirToLight), distToLight, p2.y))
			* rangedAttenuation * p2.x * data.includeAdditionalLights * data.useShadows;
		addLightShadow += lerp(0, currentShadow, step(EPSILON, p2.x) * p2.x);
		currentShadow = lerp(1, currentShadow, data.useShadows * p2.x);
		currentColor *= currentShadow;
	#endif

		// Specular
	#ifdef STANDARD_SPECULAR
		half3 currentSpecular = p1.rgb * CalculateBlinnPhongSpecular(
			data.specularSize,
			data.specularGlossiness,
			data.specularIntensity,
			normals,
			rayNormalizedDirection,
			normalize(rayHitPosition - p0.xyz),
			rangedAttenuation, p0.a) * rangedAttenuation;
			#ifdef STANDARD_SHADOWS
				currentSpecular *= currentShadow;
			#endif
			addLightSpecular += currentSpecular * data.includeAdditionalLights;
	#endif

		addLightDiffuse += currentColor * data.includeAdditionalLights;
	}

	diffuse += lerp(0, addLightDiffuse * addLightShadow, data.includeAdditionalLights);
	specular += addLightSpecular;

#endif

	return sdfRGBA * half4(diffuse, 1) + half4(specular, 0);
}

half4 CalculateLitModelWrapper(in RMStandardLitData data, in Ray ray, half4 sdfRGBA, in Texture2DArray texPack, in SamplerState texPackSampler, out half3 normal)
{
	sdfRGBA = CalculateUnlitModelWrapper(data.unlitData, ray, sdfRGBA, texPack, texPackSampler, normal);
#ifdef STANDARD_LIGHTING
	return CalculateFullLightingModel(sdfRGBA, data, ray.p, ray.nd, normal);
#else
	return sdfRGBA;
#endif
}
half4 CalculateSlopeLitModelWrapper(in RMStandardSlopeLitData data, in Ray ray, half4 sdfRGBA, in Texture2DArray texPack, in SamplerState texPackSampler, out half3 normal)
{
	sdfRGBA = CalculateUnlitModelWrapper(data.litData.unlitData, ray, sdfRGBA, texPack, texPackSampler, normal);

	half3 tex = CalculateTriplanarTexture(
	sdfRGBA, ray.p, normal, 
	texPack, texPackSampler,
	data.litData.unlitData.mainAlbedoTiling, data.slopeTextureIndex, data.slopeTextureBlend);

	float scatter = pow(max(tex.r, EPSILON), max(EPSILONUP, data.slopeTextureScatter));
	sdfRGBA.rgb = lerp(sdfRGBA.rgb, tex.rgb + lerp(tex.rgb, data.litData.unlitData.colorOverride.rgb, data.litData.unlitData.colorBlend) * scatter * data.slopeTextureEmission,
		smoothstep(data.slopeCoverage - data.slopeSmoothness, data.slopeCoverage + data.slopeSmoothness, dot(half3(0, 1, 0), normal) * scatter)
		* data.litData.unlitData.mainAlbedoOpacity);

#ifdef STANDARD_LIGHTING
	sdfRGBA = CalculateFullLightingModel(sdfRGBA, data.litData, ray.p, ray.nd, normal);
#endif

	return sdfRGBA;
}

// --------------------- Registered Core Method Contents
half4 CalculateSlopeLitModel(in RMStandardSlopeLitData data, in Ray ray, half4 sdfRGBA)
{
	half3 normals;
	return CalculateSlopeLitModelWrapper(data, ray, sdfRGBA, RMStandardSlopeLitDataTextures, sampler_RMStandardSlopeLitDataTextures, normals);
}

// ------------------- PER-OBJECT MATERIAL RENDERER ----------------------
half4 RM_PerObjRenderMaterials(in Ray ray, half4 sdfRGBA, in int sdfMaterialType, in int sdfMaterialInstance)
{
    half4 sdfRef = sdfRGBA;
    sdfRGBA = sdfMaterialType != 0 ? sdfRGBA : CalculateSlopeLitModel(RMStandardSlopeLitDataInstance[sdfMaterialInstance], ray, sdfRef);

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

    return sdfRGBA;
}
// -----------------------------------------------------------------------


// ------------------- GLOBAL POST MATERIAL RENDERER ----------------------
half4 RM_GlobalPostRenderMaterials(in Ray ray, half4 sdfRGBA, in int temp0, in int temp1)
{

    return sdfRGBA;
}
// -----------------------------------------------------------------------
