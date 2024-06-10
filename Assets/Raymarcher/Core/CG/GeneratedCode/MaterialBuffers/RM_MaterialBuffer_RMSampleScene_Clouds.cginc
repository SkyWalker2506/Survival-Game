// --------------------- Registered Global Keywords
#pragma shader_feature_fragment STANDARD_NOISE_EDGESMOOTHNESS

// --------------------- Registered Global Uniforms

// --------------------- Registered Structured/Unpacked Data Containers
struct RMStandardNoiseData
{
    half useColorOverride;
    half4 colorOverride;
    half colorBlend;
    half absorptionStep;
    half volumeStep;
    half noiseScale;
    half2 noiseTiling;
    half fillOpacity;
    half noiseDensity;
    half noiseSmoothness;
    half depthAbsorption;
    half3 edgeSmoothnessWorldPivot;
    half edgeCoverage;
    half edgeSmoothness;
    half4 edgeSize;
    half sceneDepthOffset;
    half includeAdditionalLights;
};

StructuredBuffer<RMStandardNoiseData> RMStandardNoiseDataInstance;

// --------------------- Registered Texture Containers
Texture2DArray RMStandardNoiseDataTextures;
SamplerState sampler_RMStandardNoiseDataTextures;

// --------------------- Registered Method Contents

float NHash21(float2 n)
{
	return frac(sin(dot(n, float2(12.9898, 4.1414))) * 43758.5453);
}

float NFractRoot(float2 p)
{
	float2 ip = floor(p);
	float2 u = frac(p);
	u = u * u * (3.0 - 2.0 * u);
	float res = lerp(lerp(NHash21(ip), 
		NHash21(ip + float2(1.0, 0.0)), u.x),
		lerp(NHash21(ip + float2(0.0, 1.0)),
			NHash21(ip + float2(1.0, 1.0)), u.x), u.y);
	return res * res;
}

float NNoiseRoot(float2 p, in float2 tiling)
{
	static const float2 shifting = float2(-0.60, 0.80);
	float f = 0.0;
	f += 0.500 * NFractRoot(p + tiling * _Time.x);
	p = p * shifting * 2.02;
	f += 0.03125 * NFractRoot(p);
	p = shifting * p * 1.1;
	f += 0.250 * NFractRoot(p);
	p = shifting * p * 1.2;
	f += 0.015625 * NFractRoot(p + sin(_Time.x));
	return f / 0.96875;
}

float GetNoiseOctave(in float2 p, in float2 tiling)
{
	return NNoiseRoot(p + NNoiseRoot(p + NNoiseRoot(p, tiling), tiling), tiling);
}

half4 CalculateVolNoiseModelWrapper(in RMStandardNoiseData data, in Ray ray, float4 sdfRGBA)
{
	half4 initialColor = lerp(sdfRGBA, data.colorOverride, data.useColorOverride * data.colorBlend);

	float alphaAccum = 0;
	float3 colorAccum = float3(1, 1, 1);

	float3 cp = ray.p;

	static const uint INNER_SAMPLES = 32;
	float absorbStep = data.absorptionStep / INNER_SAMPLES;
	float volumeStep = data.volumeStep;
	float noiseScale = data.noiseScale;

#ifndef RAYMARCHER_SCENE_DEPTH
	static const uint iterations = INNER_SAMPLES;
#else
	float depth = ray.sceneDepth;
	depth /= length(_WorldSpaceCameraPos) + data.sceneDepthOffset;
	uint iterations = clamp(floor(depth * INNER_SAMPLES), 8, INNER_SAMPLES);
#endif

	for (uint x = 0; x < iterations; x++)
	{
		float cSample = 
			GetNoiseOctave(cp.xy * data.noiseScale, data.noiseTiling) * 
			GetNoiseOctave(cp.xz * data.noiseScale, data.noiseTiling) * 
			GetNoiseOctave(cp.yz * data.noiseScale, data.noiseTiling);

		alphaAccum += pow(cSample, max(EPSILON, data.noiseDensity)) * absorbStep;
		cp += ray.nd * volumeStep;

#ifdef RAYMARCHER_LIGHT_COUNT
	#ifdef RAYMARCHER_ADDITIONAL_LIGHTS
			[unroll(RAYMARCHER_LIGHT_COUNT)] for (uint i = 0; i < RAYMARCHER_LIGHT_COUNT; i += 3)
			{
				// Add-Light data
				half4 p0 = RaymarcherAddLightsData[i];		// xyz = light position, w = light intensity
				half4 p1 = RaymarcherAddLightsData[i + 1];	// rgb = light color, w = light range
				half4 p2 = RaymarcherAddLightsData[i + 2];	// x = light shadow intensity, y = light shadow attenuation offset
				p0.w = abs(p0.w);
				p1.w = abs(p1.w);

				float3 dirToLight = p0.xyz - cp;
				float3 normalizedDirToLight = normalize(dirToLight);
				float distToLight = length(dirToLight);

				// Attenuation & intensity
				float rangedAttenuation = saturate(exp(-distToLight / (p1.w / 2.0))) * p0.w;
				colorAccum += p1.rgb * rangedAttenuation * data.includeAdditionalLights;
			}
	#endif
#endif
	}

	float result = exp(-pow(alphaAccum, max(EPSILON, data.noiseSmoothness)));

	sdfRGBA.a *= max(data.fillOpacity, result);

#ifdef STANDARD_NOISE_EDGESMOOTHNESS
	float edge = (1 - smoothstep(data.edgeCoverage - data.edgeSmoothness, data.edgeCoverage + data.edgeSmoothness,
		length(cp - data.edgeSmoothnessWorldPivot) - data.edgeSize.x));

	float3 offpc = abs(cp - data.edgeSmoothnessWorldPivot);
	float edgeRL = (1 - smoothstep(data.edgeSize.x * data.edgeCoverage - data.edgeSmoothness, data.edgeSize.x * data.edgeCoverage + data.edgeSmoothness, offpc.x));
	float edgeTB = (1 - smoothstep(data.edgeSize.y * data.edgeCoverage - data.edgeSmoothness, data.edgeSize.y * data.edgeCoverage + data.edgeSmoothness, offpc.y));
	float edgeFB = (1 - smoothstep(data.edgeSize.z * data.edgeCoverage - data.edgeSmoothness, data.edgeSize.z * data.edgeCoverage + data.edgeSmoothness, offpc.z));

	edge = lerp(1, lerp(edge, edgeRL * edgeTB * edgeFB, data.edgeSize.w - 1), step(EPSILON, data.edgeSize.w));

	sdfRGBA.a *= edge;
#endif

	sdfRGBA.rgb = initialColor * pow(alphaAccum, max(EPSILON, data.depthAbsorption)) * (colorAccum * result);

	return sdfRGBA;
}

// --------------------- Registered Core Method Contents
half4 CalculateVolNoiseModel(in RMStandardNoiseData data, in Ray ray, half4 sdfRGBA)
{
	return CalculateVolNoiseModelWrapper(data, ray, sdfRGBA);
}

// ------------------- PER-OBJECT MATERIAL RENDERER ----------------------
half4 RM_PerObjRenderMaterials(in Ray ray, half4 sdfRGBA, in int sdfMaterialType, in int sdfMaterialInstance)
{
    half4 sdfRef = sdfRGBA;
    sdfRGBA = sdfMaterialType != 0 ? sdfRGBA : CalculateVolNoiseModel(RMStandardNoiseDataInstance[sdfMaterialInstance], ray, sdfRef);

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
