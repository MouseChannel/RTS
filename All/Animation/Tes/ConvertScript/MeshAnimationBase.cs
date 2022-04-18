//----------------------------------------------
// Mesh Animator
// Flick Shot Games
// http://www.flickshotgames.com
//----------------------------------------------

using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace FSG.MeshAnimator
{
    [System.Serializable]
    public struct exposedItemsData{
        public Mesh mesh;
        public float3x3 position;
        
        public string fatherName;
        public int fatherIndex;


    }
    [System.Serializable]
    public abstract class MeshAnimationBase : ScriptableObject, IMeshAnimation
    {
        // [HideInInspector]
        public int Mode { get; set; }
        // public int a;
        public string AnimationName { get { return animationName; } }
        public string[] ExposedTransforms { get { return exposedTransforms; } }
        public abstract MeshFrameDataBase[] Frames { get; }
        public MeshAnimationEvent[] Events { get { return events; } }
        public abstract int TotalFrames { get; }
        public float PlaybackSpeed { get { return playbackSpeed; } }
        public float Length { get { return length; } }
        public WrapMode WrapMode { get { return wrapMode; } }
        public RootMotionMode RootMotionMode { get { return rootMotionMode; } }
        public int FrameSkip { get { return frameSkip; } }
        public int VertexCount { get { return vertexCount; } }

        public string animationName;
        public float playbackSpeed = 1;
        public WrapMode wrapMode = WrapMode.Default;
        public MeshAnimationEvent[] events;
        public float length;
        [HideInInspector]
        public int frameSkip = 1;
        [HideInInspector]
        public RootMotionMode rootMotionMode = RootMotionMode.None;
        // [HideInInspector]
        public string[] exposedTransforms;
        public List<Vector3> exposedPosition = new List<Vector3>();
        // [HideInInspector]
        public int vertexCount;

        // public List<Mesh> exposedItems;
        public List<float3x3> exposedItemsPosition = new List<float3x3>();
        public List<exposedItemsData> exposedItemsDatas = new List<exposedItemsData>();

        protected virtual void OnEnable()
        {
            if (string.IsNullOrEmpty(animationName))
                animationName = name;
        }
        public void FireEvents(GameObject eventReciever, int frame)
        {
            for (int i = 0; i < events.Length; i++)
            {
                var e = events[i];
                if (e.frame == frame)
                {
                    e.FireEvent(eventReciever);
                }
            }
        }
        public bool IsName(string animationName)
        {
            if (animationName.Length != this.animationName.Length)
                return false;
            return string.CompareOrdinal(animationName, this.animationName) == 0;
        }
        public virtual MeshFrameDataBase GetNearestFrame(int frame)
        {
            var frameData = Frames;
            bool needsInterp = frame % frameSkip != 0;
            if (needsInterp) // find the closest actual frame
            {
                float nearFrame = frame / (float)frameSkip;
                frame = Mathf.RoundToInt(nearFrame);
            }
            int f = frame / frameSkip;
            if (frameData.Length <= f)
                f = frameData.Length - 1;
            else if (frame < 0)
                f = 0;
            return frameData[f];
        }
        public float GetInterpolatingFrames(int frame, out MeshFrameDataBase previousFrame, out MeshFrameDataBase nextFrame)
        {
            bool needsInterp = frame % frameSkip != 0;
            var frameData = Frames;
            if (!needsInterp)
            {
                int frameIndex = frame / frameSkip;
                previousFrame = frameData[frameIndex];
                if (frameIndex + 1 >= frameData.Length)
                    nextFrame = frameData[0];
                else
                    nextFrame = frameData[frameIndex + 1];
                return 0;
            }
            float framePerc = (float)frame / (float)(frameSkip * frameData.Length);
            int skipFrame = (int)(framePerc * frameData.Length);

            float prevFramePerc = (float)skipFrame / (float)frameData.Length;
            float nextFramePerc = Mathf.Clamp01((float)(skipFrame + 1) / (float)frameData.Length);

            previousFrame = frameData[skipFrame];
            if (skipFrame + 1 >= frameData.Length)
                nextFrame = frameData[0];
            else
                nextFrame = frameData[skipFrame + 1];

            float lerpVal = Mathf.Lerp(0, 1, (framePerc - prevFramePerc) / (nextFramePerc - prevFramePerc));
            return lerpVal;
        }

        public abstract void SetFrameData(int frame, MeshFrameDataBase frameData);
        public abstract void SetFrameData(MeshFrameDataBase[] frameData);
        public abstract void DisplayFrame(IMeshAnimator meshAnimator, int frame, int previousFrame);
        public abstract void GenerateFrame(Mesh baseMesh, int frame);
        public abstract void GenerateFrames(Mesh baseMesh);
        public abstract void Reset();
#if UNITY_EDITOR
        public abstract void CreateBakedAssets(string path, List<List<Vector3>> meshFramePositions, List<List<Vector3>> meshNormalPositions);
        public abstract void CompleteBake(IMeshAnimation[] animations, params object[] parameters);
#endif
    }
}