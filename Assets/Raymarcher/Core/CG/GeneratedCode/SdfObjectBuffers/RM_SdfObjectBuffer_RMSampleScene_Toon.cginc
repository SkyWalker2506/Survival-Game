// This code has been generated by the RMConvertor. Please do not attempt any changes


// SDF methods (sdf object sources & modifiers)
float2 GetCubeSdf(float3 p, half color, half cubeRoundness, half3 cubeSize)
{
float result;
result = length(max(abs(p) - abs(cubeSize) + cubeRoundness, 0.0)) - cubeRoundness;
return float2 (result,color);
}

float2 GetSphereSdf(float3 p, half color, half2 sphereSdfData)
{
float result;
p.y -= clamp(p.y, 0.0, sphereSdfData.y);
result = length(p) - sphereSdfData.x;
return float2 (result,color);
}

float2 GetTorusSdf(float3 p, half color, half torusThickness, half torusRadius, half torusHeight)
{
float result;
p.y -= clamp(p.y, 0.0, torusHeight);
result = length(float2(length(p.xz) - torusRadius, p.y)) - torusThickness;
return float2 (result,color);
}

float2 GetConeSdf(float3 p, half color, half coneHeight, half coneSize)
{
float result;
result = max(dot(float2(0.5, coneSize), float2(length(p.xz), p.y - coneHeight)), -p.y - coneHeight);
return float2 (result,color);
}


// SDF object variables
struct SdfInstancesContainer
{
half4x4 modelData;
};
StructuredBuffer<SdfInstancesContainer> SdfInstances;
uniform half cubeRoundness0;
uniform half3 cubeSize0;
uniform half2 sphereSdfData1;
uniform half torusThickness2;
uniform half torusRadius2;
uniform half torusHeight2;
uniform half coneHeight3;
uniform half coneSize3;
uniform half cubeRoundness4;
uniform half3 cubeSize4;

float4 SdfObjectBuffer(in float3 p)
{
// SDF object declarations
    float4 obj0 = float4 (GetCubeSdf(RM_TRANS(p,SdfInstances[0].modelData),SdfInstances[0].modelData[3].x,cubeRoundness0,cubeSize0),0,0);
    float4 obj1 = float4 (GetSphereSdf(RM_TRANS(p,SdfInstances[1].modelData),SdfInstances[1].modelData[3].x,sphereSdfData1),0,0);
    float4 obj2 = float4 (GetTorusSdf(RM_TRANS(p,SdfInstances[2].modelData),SdfInstances[2].modelData[3].x,torusThickness2,torusRadius2,torusHeight2),0,0);
    float4 obj3 = float4 (GetConeSdf(RM_TRANS(p,SdfInstances[3].modelData),SdfInstances[3].modelData[3].x,coneHeight3,coneSize3),0,0);
    float4 obj4 = float4 (GetCubeSdf(RM_TRANS(p,SdfInstances[4].modelData),SdfInstances[4].modelData[3].x,cubeRoundness4,cubeSize4),0,0);
// SDF modifier groups

// Smooth Unions
    float4 objGroup0 = GroupSmoothUnion(obj0,obj1,RaymarcherGlobalSdfObjectSmoothness);
    float4 objGroup1 = GroupSmoothUnion(objGroup0,obj2,RaymarcherGlobalSdfObjectSmoothness);
    float4 objGroup2 = GroupSmoothUnion(objGroup1,obj3,RaymarcherGlobalSdfObjectSmoothness);
    float4 objGroup3 = GroupSmoothUnion(objGroup2,obj4,RaymarcherGlobalSdfObjectSmoothness);
    return objGroup3;
}
