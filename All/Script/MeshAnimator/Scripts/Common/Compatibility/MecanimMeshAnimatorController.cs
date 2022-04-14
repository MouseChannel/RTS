//----------------------------------------------
// Mesh Animator
// Flick Shot Games
// http://www.flickshotgames.com
//----------------------------------------------

using System.Collections.Generic;
using UnityEngine;

namespace FSG.MeshAnimator
{
    public class MecanimMeshAnimatorController : MonoBehaviour
    {
        public Animator animator;
        public MeshAnimatorBase meshAnimator;

        private Dictionary<int, string> _animationHashes = new Dictionary<int, string>();
        private string _currentAnimationName = string.Empty;

        protected virtual void Awake()
        {
            if (meshAnimator == null)
            {
                Debug.LogError("MecanimMeshAnimatorController.meshAnimator is null", this);
                return;
            }
            for (int i = 0; i < meshAnimator.Animations.Length; i++)
            {
                string animationName = meshAnimator.Animations[i].AnimationName;
#if UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_5 || UNITY_4_6
				animHashes.Add(Animator.StringToHash(animationName), animationName);
#else
                _animationHashes.Add(Animator.StringToHash("Base Layer." + animationName), animationName);
#endif
            }
        }
        protected virtual void LateUpdate()
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
#if UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_5 || UNITY_4_6
			int id = stateInfo.nameHash;
#else
            int id = stateInfo.fullPathHash;
#endif
            if (_animationHashes.ContainsKey(id))
            {
                if (_currentAnimationName != _animationHashes[id])
                {
                    _currentAnimationName = _animationHashes[id];
                    meshAnimator.Play(_animationHashes[id]);
                }
            }
        }
    }
}