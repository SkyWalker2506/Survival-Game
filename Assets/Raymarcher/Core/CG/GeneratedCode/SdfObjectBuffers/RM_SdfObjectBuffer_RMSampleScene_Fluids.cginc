// This code has been generated by the RMConvertor. Please do not attempt any changes


// SDF methods (sdf object sources & modifiers)
half2 GetTexture3DVolumeSdf(half3 p, half color, half3 volumeSize, half volumeAmplifier, half volumePrecision, sampler3D volumeTexture)
{
half result;
half3 volCoords = (p.xyz / volumeSize.xyz + 1.0) * 0.5;
float volTex = RM_SAMPLE_TEXTURE3D(volumeTexture, volCoords).r;
float3 d = abs(p) - volumeSize;
float bsdf = min(max(d.x, max(d.y, d.z)), 0.0) + length(max(d, 0.0));

result = max(bsdf, 1 - volTex * volumeAmplifier) / max(EPSILONZEROFIVE, volumePrecision / volumeSize.x);
return half2 (result,color);
}


// SDF object variables
struct SdfInstancesContainer
{
half4 modelData;
};
StructuredBuffer<SdfInstancesContainer> SdfInstances;
uniform half3 volumeSize0;
uniform half volumeAmplifier0;
uniform half volumePrecision0;
uniform sampler3D volumeTexture0;
uniform half3 volumeSize1;
uniform half volumeAmplifier1;
uniform half volumePrecision1;
uniform sampler3D volumeTexture1;
uniform half3 volumeSize2;
uniform half volumeAmplifier2;
uniform half volumePrecision2;
uniform sampler3D volumeTexture2;

half2 SdfObjectBuffer(in half3 p)
{
// SDF object declarations
    half2 obj0 = half2 (GetTexture3DVolumeSdf(RM_TRANS(p,SdfInstances[0].modelData.xyz),SdfInstances[0].modelData.w,volumeSize0,volumeAmplifier0,volumePrecision0,volumeTexture0));
    half2 obj1 = half2 (GetTexture3DVolumeSdf(RM_TRANS(p,SdfInstances[1].modelData.xyz),SdfInstances[1].modelData.w,volumeSize1,volumeAmplifier1,volumePrecision1,volumeTexture1));
    half2 obj2 = half2 (GetTexture3DVolumeSdf(RM_TRANS(p,SdfInstances[2].modelData.xyz),SdfInstances[2].modelData.w,volumeSize2,volumeAmplifier2,volumePrecision2,volumeTexture2));
// SDF modifier groups

// Smooth Unions
    half2 objGroup0 = GroupSmoothUnion(obj0,obj1,RaymarcherGlobalSdfObjectSmoothness);
    half2 objGroup1 = GroupSmoothUnion(objGroup0,obj2,RaymarcherGlobalSdfObjectSmoothness);
    return objGroup1;
}
