//////////////////////////////////////////////////////////////////////////
// The classical shadow map renderer
//
#include "Inc/Global.hlsl"
#include "Inc/ShadowMap.hlsl"

cbuffer	cbObject	: register( b10 )
{
	float4x4	_Local2World;
};

// Scene vertex format
struct	VS_IN
{
	float3	Position	: POSITION;
// 	float3	Normal		: NORMAL;
// 	float3	Tangent		: TANGENT;
// 	float3	BiTangent	: BITANGENT;
// 	float2	UV			: TEXCOORD0;
};

struct	PS_IN
{
	float4	__Position	: SV_POSITION;
};


///////////////////////////////////////////////////////////
// Shadow Map rendering
PS_IN	VS( VS_IN _In )
{
	float4	WorldPosition = mul( float4( _In.Position, 1.0 ), _Local2World );

	PS_IN	Out;
	Out.__Position = World2ShadowMapProj( WorldPosition );

	return Out;
}

// Won't be used anyway!
float	PS( PS_IN _In ) : SV_TARGET0
{
	return 0.0;
}
