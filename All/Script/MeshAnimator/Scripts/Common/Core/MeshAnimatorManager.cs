//----------------------------------------------
// Mesh Animator
// Flick Shot Games
// http://www.flickshotgames.com
//----------------------------------------------

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace FSG.MeshAnimator
{
    public class MeshAnimatorManager : MonoBehaviour
    {
        public static int AnimatorCount { get { if (Instance) return _animators.Count; return 0; } }
        public static MeshAnimatorManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<MeshAnimatorManager>();
                    if (_instance == null)
                    {
                        _instance = new GameObject("MeshAnimatorManager").AddComponent<MeshAnimatorManager>();
                    }
                }
                return _instance;
            }
        }

        private static AnimatorUpdateMode _mode = AnimatorUpdateMode.Normal;
        private static MeshAnimatorManager _instance = null;
        private static List<IMeshAnimator> _animators = new List<IMeshAnimator>(100);
        private static List<IMeshAnimator> _addAnimators = new List<IMeshAnimator>(100);
        private static List<IMeshAnimator> _removeAnimators = new List<IMeshAnimator>(100);

        public static void AddAnimator(IMeshAnimator animator)
        {
            if (Instance)
            {
                _addAnimators.Add(animator);
            }
        }
        public static void RemoveAnimator(IMeshAnimator animator)
        {
            _removeAnimators.Add(animator);
        }
        public static void SetUpdateMode(AnimatorUpdateMode updateMode)
        {
            _mode = updateMode;
            if (_mode == AnimatorUpdateMode.UnscaledTime && _instance != null)
            {
                _instance.StartCoroutine(_instance.UnscaledUpdate());
            }
        }

        private void Awake()
        {
            if (_mode == AnimatorUpdateMode.UnscaledTime)
                StartCoroutine(UnscaledUpdate());
        }
        private IEnumerator UnscaledUpdate()
        {
            while (enabled && _mode == AnimatorUpdateMode.UnscaledTime)
            {
                UpdateTick(Time.realtimeSinceStartup);
                yield return null;
            }
        }
        private void Update()
        {
            if (_mode == AnimatorUpdateMode.Normal)
                UpdateTick(Time.time);
        }
        private void FixedUpdate()
        {
            if (_mode == AnimatorUpdateMode.AnimatePhysics)
                UpdateTick(Time.time);
        }
        private void UpdateTick(float time)
        {
            if (_addAnimators.Count > 0)
            {
                _animators.AddRange(_addAnimators);
                _addAnimators.Clear();
            }

            if (_removeAnimators.Count > 0)
            {
                for (int i = 0; i < _removeAnimators.Count; i++)
                {
                    IMeshAnimator remove = _removeAnimators[i];
                    _animators.Remove(remove);
                }
                _removeAnimators.Clear();
            }

            int count = _animators.Count;
            for (int i = 0; i < count; i++)
            {
                IMeshAnimator animator = _animators[i];
                if (time >= animator.NextTick)
                {
                    try
                    {
                        animator.UpdateTick(time);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }
        }
    }
}