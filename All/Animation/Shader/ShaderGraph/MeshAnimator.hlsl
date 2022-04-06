//UNITY_SHADER_NO_UPGRADE

float1 GetPixelOffset(inout float1 textureIndex, float4 animInfo, float4 animTimeInfo)
{
	float1 normalizedTime = (_Time.y - animTimeInfo.z) / (animTimeInfo.w - animTimeInfo.z);
	normalizedTime = normalizedTime - floor(normalizedTime);
	float1 currentFrame = min(floor(normalizedTime * animTimeInfo.y), animTimeInfo.y - 1);
	float1 vertexCount = animInfo.y;
	float1 textureSizeX = animInfo.z;
	float1 textureSizeY = animInfo.w;

	float1 framesPerTexture = floor((textureSizeX * textureSizeY) / (vertexCount * 2));
    float1 localOffset = floor(currentFrame / framesPerTexture);
    textureIndex = floor(textureIndex + localOffset);
	float1 frameOffset = floor(currentFrame % framesPerTexture);
    float1 pixelOffset = vertexCount * 2 * frameOffset;
	return pixelOffset;
}

float3 GetUVPos(uint vertexIndex, float1 textureIndex, float1 pixelOffset, float1 textureSizeX, float1 textureSizeY, uint offset)
{
	uint vertexOffset = pixelOffset + (vertexIndex * 2);
	vertexOffset += offset;
	float1 offsetX = floor(vertexOffset / textureSizeX);
	float1 offsetY = vertexOffset - (offsetX * textureSizeY);
	float3 uvPos = float3(offsetX / textureSizeX, offsetY / textureSizeY, textureIndex);
	return uvPos;
}

float3 GetAnimationUVPosition(uint vertexIndex, float1 textureIndex, float4 animInfo, float4 animTimeInfo, uint offset)
{
    float1 pixelOffset = GetPixelOffset(textureIndex, animInfo, animTimeInfo);
	return GetUVPos(vertexIndex, textureIndex, pixelOffset, animInfo.z, animInfo.w, offset);
}

float3 GetCrossfadeUVPosition(uint vertexIndex, float1 textureIndex, float4 animInfo)
{
	float1 pixelOffset = animInfo.x;
	return GetUVPos(vertexIndex, textureIndex, pixelOffset, animInfo.z, animInfo.w, 0);
}

float3 DecodeNegativeVectors(float3 positionData)
{
	positionData.x = (positionData.x - 0.5) * 2;
	positionData.y = (positionData.y - 0.5) * 2;
	positionData.z = (positionData.z - 0.5) * 2;
	return positionData;
}

float3 ApplyAnimationScalar(float3 positionData, float4 animScalar)
{
	positionData = DecodeNegativeVectors(positionData);
	positionData *= animScalar.xyz;
	return positionData;
}

float3 ApplyMeshAnimation(
	float3 position,
	float1 vertexId,
	float1 index,
	float4 animInfo,
	float4 animTimeInfo,
	float4 animScalar,
	float1 crossfadeStartTime,
	float1 crossfadeEndTime,
	float1 cfIndex,
	float4 cfAnimInfo,
	float4 cfAnimScalar,
	Texture2DArray animTextures,
	SamplerState samplerState)
{
	if (index >= 0)
	{
		float3 uvPos = GetAnimationUVPosition(vertexId, index, animInfo, animTimeInfo, 0);
		float3 positionData = SAMPLE_TEXTURE2D_ARRAY_LOD(animTextures, samplerState, uvPos.xy, uvPos.z, 0).xyz;
		positionData = ApplyAnimationScalar(positionData, animScalar);

		if (_Time.y < crossfadeEndTime)
		{
			uvPos = GetCrossfadeUVPosition(vertexId, cfIndex, cfAnimInfo);
			float3 crossfadePositionData = SAMPLE_TEXTURE2D_ARRAY_LOD(animTextures, samplerState, uvPos.xy, uvPos.z, 0).xyz;
			crossfadePositionData = ApplyAnimationScalar(crossfadePositionData, cfAnimScalar);
		 
			float lerpValue = (_Time.y - crossfadeStartTime) / (crossfadeEndTime - crossfadeStartTime);
			positionData = lerp(crossfadePositionData, positionData, lerpValue);
		}
		position = positionData;
	}
	return position;
}

float3 GetAnimatedMeshNormal(
	float3 normal,
	uint vertexId,
	float1 index,
	float4 animInfo,
	float4 animTimeInfo,
	Texture2DArray animTextures,
	SamplerState samplerState)
{
	if (index >= 0)
	{
		float3 uvPos = GetAnimationUVPosition(vertexId, index, animInfo, animTimeInfo, 1);
		float3 normalData = SAMPLE_TEXTURE2D_ARRAY_LOD(animTextures, samplerState, uvPos.xy, uvPos.z, 0).xyz;
		normalData = DecodeNegativeVectors(normalData);
		if (normalData.x != 0 || normalData.y != 0 || normalData.z != 0)
			normal = normalData;
	}
	return normal;
}

void ApplyMeshAnimationValues_float(
	float3 vertexPosition,
	float3 vertexNormal,
	float4 animTimeInfo, 
	Texture2DArray animTextures,
	float1 animTextureIndex,
	float4 animInfo, 
	float4 animScalar, 
	float1 crossfadeAnimTextureIndex, 
	float4 crossfadeAnimInfo, 
	float4 crossfadeAnimScalar, 
	float1 crossfadeStartTime,  
	float1 crossfadeEndTime,  
	float1 vertexIndex,
	SamplerState samplerState,
	out float3 outputVertexPosition,
	out float3 outputVertexNormal)
{
	outputVertexPosition = ApplyMeshAnimation(
		vertexPosition,
		vertexIndex,
		animTextureIndex,
		animInfo,
		animTimeInfo,
		animScalar,
		crossfadeStartTime,
		crossfadeEndTime,
		crossfadeAnimTextureIndex,
		crossfadeAnimInfo,
		crossfadeAnimScalar,
		animTextures,
		samplerState);

	outputVertexNormal = GetAnimatedMeshNormal(
		vertexNormal,
		vertexIndex,
		animTextureIndex,
		animInfo,
		animTimeInfo,
		animTextures,
		samplerState);
}