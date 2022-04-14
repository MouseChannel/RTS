//----------------------------------------------
// Mesh Animator
// Flick Shot Games
// http://www.flickshotgames.com
//----------------------------------------------

using UnityEngine;

namespace FSG.MeshAnimator
{
	[System.Serializable]
	public class MeshFrameDataBase
	{
		public Matrix4x4[] exposedTransforms;
        public int ee;
        [System.NonSerialized]
		public Vector3 rootMotionPosition;
		[System.NonSerialized]
		public Quaternion rootMotionRotation;
	 
	}
}