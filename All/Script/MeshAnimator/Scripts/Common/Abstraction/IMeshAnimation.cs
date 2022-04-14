using UnityEngine;

namespace FSG.MeshAnimator
{
    public interface IMeshAnimation
    {
        string AnimationName { get; }
        string[] ExposedTransforms { get; }
        MeshFrameDataBase[] Frames { get; }
        MeshAnimationEvent[] Events { get; }
        int TotalFrames { get; }
        float PlaybackSpeed { get; }
        float Length { get; }
        WrapMode WrapMode { get; }
        RootMotionMode RootMotionMode { get; }
        int FrameSkip { get; }
        int VertexCount { get; }

        void DisplayFrame(IMeshAnimator meshAnimator, int frame, int previousFrame);
        void GenerateFrame(Mesh baseMesh, int frame);
        void GenerateFrames(Mesh baseMesh);
        void Reset();
        void FireEvents(GameObject eventReciever, int frame);
        MeshFrameDataBase GetNearestFrame(int frame);
        float GetInterpolatingFrames(int frame, out MeshFrameDataBase previousFrame, out MeshFrameDataBase nextFrame);
        bool IsName(string name);
    }
}