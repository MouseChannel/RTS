//----------------------------------------------
// Mesh Animator
// Flick Shot Games
// http://www.flickshotgames.com
//----------------------------------------------

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FSG.MeshAnimator.ShaderAnimated
{
    [System.Serializable]
    public class ShaderMeshAnimation : MeshAnimationBase
    {
        public override MeshFrameDataBase[] Frames { get { return frameData; } }
        public override int TotalFrames { get { return frameData.Length; } }

        // [HideInInspector]
        public MeshFrameDataBase[] frameData;
        // [HideInInspector]
        public Texture2D[] textures;
        // [HideInInspector]
        public Vector3 animScalar = Vector3.one;
        // [HideInInspector]
        public Vector2Int textureSize;
        // [HideInInspector]
        public int textureCount;

        public override MeshFrameDataBase GetNearestFrame(int frame)
        {
            if (frameData.Length <= frame)
                frame = frameData.Length - 1;
            else if (frame < 0)
                frame = 0;
            return frameData[frame];
        }
        public override void DisplayFrame(IMeshAnimator meshAnimator, int frame, int previousFrame) { }
        public override void GenerateFrame(Mesh baseMesh, int frame) { }
        public override void GenerateFrames(Mesh baseMesh) { }
        public override void Reset() { }
        // public ExposedFramePositionData[] OutputFrameData(){
            
        // }

        public override void SetFrameData(int frame, MeshFrameDataBase frameData)
        {
            this.frameData[frame] = frameData;
        }
        public override void SetFrameData(MeshFrameDataBase[] frameData)
        {
            this.frameData = frameData;
        }
#if UNITY_EDITOR
        [System.NonSerialized]
        private List<List<Vector3>> frameBakeData;
        private List<List<Vector3>> normalBakeData;
        public override void CreateBakedAssets(string path, List<List<Vector3>> framePositions, List<List<Vector3>> frameNormals)
        {
            vertexCount = framePositions[0].Count;
            frameBakeData = framePositions;
            normalBakeData = frameNormals;
            int TEX_SIZE = 1024;
            int frameVertexCount = vertexCount * frameBakeData.Count * 2;
            while (TEX_SIZE * TEX_SIZE > frameVertexCount)
            {
                if (TEX_SIZE * TEX_SIZE / 2 < frameVertexCount)
                    break;
                TEX_SIZE /= 2;
            }
            textureSize = new Vector2Int(TEX_SIZE, TEX_SIZE);
        }
        public override void CompleteBake(IMeshAnimation[] animations, params object[] parameters)
        {
            float compressionAccuracy = (float)parameters[1];
            if (frameBakeData.Count > 0)
                vertexCount = frameBakeData[0].Count;
            double[][] offsets = new double[vertexCount * frameBakeData.Count][];
            double[] scaler = new double[3];
            // generate texture
            for (int frameIndex = 0; frameIndex < frameBakeData.Count; frameIndex++)
            {
                var meshFrame = frameBakeData[frameIndex];
                var meshNormal = normalBakeData[frameIndex];
                for (int vert = 0; vert < meshFrame.Count; vert++)
                {
                    int arrayPos = (frameIndex * meshFrame.Count) + vert;
                    var framePos = meshFrame[vert];
                    Vector3 frameNormal = Vector3.zero;
                    if (meshNormal.Count > vert)
                    {
                        frameNormal = meshNormal[vert];
                    }
                    double[] data = new double[6]
                    {
                        framePos.x,
                        framePos.y,
                        framePos.z,
                        frameNormal.x,
                        frameNormal.y,
                        frameNormal.z
                    };
                    if (compressionAccuracy != 1)
                    {
                        data[0] = System.Math.Round(data[0] * compressionAccuracy) / compressionAccuracy;
                        data[1] = System.Math.Round(data[1] * compressionAccuracy) / compressionAccuracy;
                        data[2] = System.Math.Round(data[2] * compressionAccuracy) / compressionAccuracy;
                    }
                    offsets[arrayPos] = data;

                    for (int s = 0; s < 3; s++)
                    {
                        if (System.Math.Abs(data[s]) > scaler[s])
                            scaler[s] = System.Math.Abs(data[s]);
                    }
                }
            }
            animScalar = new Vector3((float)scaler[0], (float)scaler[1], (float)scaler[2]);
            List<Texture2D> bakeTextures = new List<Texture2D>();
            int xPos = 0;
            int yPos = 0;
            int textureIndex = 0;
            int frame = 0;
            int pixelsLeft = textureSize.x * textureSize.y;
            int verticesLeftInFrame = vertexCount * 2;
            for (int vert = 0; vert < offsets.Length; vert++)
            {
                double[] data = offsets[vert];
                if (data == null)
                    continue;
                for (int s = 0; s < data.Length; s++)
                {
                    if (s < 3)
                    {
                        if (scaler[s] != 0)
                            data[s] /= scaler[s];
                    }
                    // convert all negatives to positives
                    data[s] = data[s] * 0.5d + 0.5d;
                }

                for (int c = 0; c < data.Length; c += 3)
                {
                    Color color = new Color((float)data[c + 0], (float)data[c + 1], (float)data[c + 2], 1);
                    if (yPos == textureSize.y)
                    {
                        xPos++;
                        yPos = 0;
                        if (xPos == textureSize.x)
                        {
                            xPos = 0;
                            textureIndex++;
                            pixelsLeft = textureSize.x * textureSize.y;
                        }
                    }
                    if (bakeTextures.Count <= textureIndex)
                    {
                        bakeTextures.Add(new Texture2D(textureSize.x, textureSize.y, (TextureFormat)(int)parameters[0], false, false));
                    }
                    var bakeTexture = bakeTextures[textureIndex];
                    bakeTexture.SetPixel(xPos, yPos, color);
                    yPos++;

                    pixelsLeft--;
                    verticesLeftInFrame--;
                    // advance texture if whole frame next doesn't fit on it
                    if (verticesLeftInFrame == 0)
                    {
                        verticesLeftInFrame = vertexCount * 2;
                        frame++;
                        if (pixelsLeft < vertexCount * 2)
                        {
                            textureIndex++;
                            pixelsLeft = textureSize.x * textureSize.y;
                            xPos = 0;
                            yPos = 0;
                        }
                    }
                }
            }
            string path = UnityEditor.AssetDatabase.GetAssetPath(this);
            var existingTextures = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(path).Where(a => a is Texture2D).ToArray();
            for (int t = 0; t < bakeTextures.Count; t++)
            {
                bakeTextures[t].name = string.Format("{0}_{1}", this.name, t);
                foreach (var existing in existingTextures)
                    DestroyImmediate(existing, true);
                UnityEditor.AssetDatabase.AddObjectToAsset(bakeTextures[t], this);
            }
            textures = bakeTextures.ToArray();
            textureCount = textures.Length;
        }
#endif
    }
}