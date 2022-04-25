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

float3 ApplyAnimationScalar(float3 positionData, float3 animScalar)
{
	positionData = DecodeNegativeVectors(positionData);
	positionData *= animScalar;
	return positionData;
}

float3 ApplyMeshAnimation(
	float3 position,
	float1 vertexId,
	float4x4 animData,
	float4x4 crossfadeData,

	// float1 animTextureIndex1,
	// float4 animInfo1,
	// float4 animTimeInfo1,
	// float3 animScalar1,
	// float1 crossfadeStartTime1,
	// float1 crossfadeEndTime1,
	// float1 cfIndex1,
	// float4 cfAnimInfo1,
	// float3 cfAnimScalar1,
	Texture2DArray animTextures,
	SamplerState samplerState)
{

// crossfadeDate 第一行: float1 crossfadeAnimTextureIndex
// 第二行 float4 crossfadeAnimInfo
// 第三行 float3 crossfadeAnimScalar
// 第四行 float2 crossfadeStartTime 和 crossfadeEndTime


	if ((float1)animData[1] >= 0)
	{
		float1 index = (float1)animData[1];
		float4 animInfo = animData[2];
		float4 animTimeInfo = animData[0];
		float3 animScalar = (float3)animData[3];

		float1 cfIndex = (float1)crossfadeData[0];
		float4 cfAnimInfo = crossfadeData[1];
		float3 cfAnimScalar = (float3) crossfadeData[2];
		float1 crossfadeStartTime = crossfadeData[3][0];		
		float1 crossfadeEndTime = crossfadeData[3][1];


		 
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
	float4x4 animData,
	// float1 animTextureIndex,
	// float4 animInfo,
	// float4 animTimeInfo,
	Texture2DArray animTextures,
	SamplerState samplerState)
{
	if ((float1)animData[1] >= 0)
	{
		float1 index = (float1)animData[1];
		float4 animInfo = animData[2];
		float4 animTimeInfo = animData[0];
		// float3 animScalar - (float3)animData[3];

		// float1 cfIndex1 = (float1)crossfadeData[0];
		// float4 cfAnimInfo = crossfadeData[1];
		// float3 cfAnimScalar = (float3) crossfadeData[2];
		// float1 crossfadeStartTime = crossfadeData[3][0];		
		// float1 crossfadeEndTime = crossfadeData[3][1];
		


		float3 uvPos = GetAnimationUVPosition(vertexId, index, animInfo, animTimeInfo, 1);
		float3 normalData = SAMPLE_TEXTURE2D_ARRAY_LOD(animTextures, samplerState, uvPos.xy, uvPos.z, 0).xyz;
		normalData = DecodeNegativeVectors(normalData);
		if (normalData.x != 0 || normalData.y != 0 || normalData.z != 0)
			normal = normalData;
	}
	return normal;
}
bool ShouldRender(
	uint vertexId,
	float4x4 mat
){
	//列优先
	uint fromIndex = 0;
	uint endIndex = 0;
	while(mat[fromIndex + 1][endIndex / 4] != mat[fromIndex][endIndex / 4]){
		if(vertexId >= mat[fromIndex][endIndex / 4] && vertexId < mat[fromIndex + 1][endIndex / 4]){
			return true;
		}
		fromIndex = (fromIndex + 2) % 4;
		endIndex = (endIndex + 2) ;
		if(endIndex > 15 ) break;
	}
	// if(vertexId < mat[0][1]) return true;
	return false;



}
float1 GetAlpha(
	uint vertexId,
	float4x4 renderRange
){
	if(ShouldRender(vertexId, renderRange))
	{
		return 0.4;
	}
	return 0.6;

}
/*animData第一行:float4 animTimeInfo
第二行 float1 animTextureIndex
第三行 float4 animinfo
第四行 float3 animScalar

crossfadeDate 第一行: float1 crossfadeAnimTextureIndex
第二行 float4 crossfadeAnimInfo
第三行 float3 crossfadeAnimScalar
第四行 float2 crossfadeStartTime 和 crossfadeEndTime


*/

void ApplyMeshAnimationValues_float(
	float3 vertexPosition,
	float3 vertexNormal,
	Texture2DArray animTextures,
	float4x4 animData,
	float4x4 crossfadeData,

	// float4 animTimeInfo, 
	// float1 animTextureIndex,
	// float4 animInfo, 
	// float3 animScalar, 
	// float1 crossfadeAnimTextureIndex, 
	// float4 crossfadeAnimInfo, 
	// float3 crossfadeAnimScalar, 
	// float1 crossfadeStartTime,  
	// float1 crossfadeEndTime,  

	float1 vertexIndex,
	float4x4 renderRange,
	
	SamplerState samplerState,
	out float3 outputVertexPosition,
	out float3 outputVertexNormal,
	out float outputAlpha)
{
	outputVertexPosition = ApplyMeshAnimation(
		vertexPosition,
		vertexIndex,
		animData,
		crossfadeData,

		// animTextureIndex,
		// animInfo,
		// animTimeInfo,
		// animScalar,
		// crossfadeStartTime,
		// crossfadeEndTime,
		// crossfadeAnimTextureIndex,
		// crossfadeAnimInfo,
		// crossfadeAnimScalar,

		animTextures,
		samplerState);

	outputVertexNormal = GetAnimatedMeshNormal(
		vertexNormal,
		vertexIndex,
		animData,
		// crossfadeData,


		// animTextureIndex,
		// animInfo,
		// animTimeInfo,
		animTextures,
		samplerState);

	outputAlpha = GetAlpha(
		vertexIndex,
		renderRange
	);
}