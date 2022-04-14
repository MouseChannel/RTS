using UnityEngine;

namespace FSG.MeshAnimator
{
    public interface IMeshAnimator
    {
        MeshRenderer MeshRenderer { get;}
        MeshFilter MeshFilter { get; }
        IMeshAnimation[] Animations { get; }
        float NextTick { get; }
        System.Action<string> OnAnimationFinished { get; set; }
        System.Action OnFrameUpdated { get; set; }
        System.Action<bool> OnVisibilityChanged { get; set; }

        void UpdateTick(float time);
        void Play();
        void Play(string animationName, float normalizedTime = -1);
        void Play(int animationIndex);
        IMeshAnimation GetClip(string animationName);
        void SetTime(float time, bool instantUpdate = false);
        void SetTimeNormalized(float time, bool instantUpdate = false);
        bool IsPlaying();
    }
}