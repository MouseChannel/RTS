using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace FSG.MeshAnimator
{
    public struct ExposedTransformCrossfade
    {
        public struct LerpMatrix4x4Job : IJobParallelFor
        {
            [ReadOnly] public NativeArray<Matrix4x4> from;
            [ReadOnly] public NativeArray<Matrix4x4> to;
            [ReadOnly] public float delta;
            public NativeArray<Matrix4x4> output;
            public void Execute(int i)
            {
                Matrix4x4 m = output[i];
                for (int j = 0; j < 16; j++)
                    m[j] = Mathf.Lerp(from[i][j], to[i][j], delta);
                output[i] = m;
            }
        }
        public int framesNeeded;
        public int currentFrame;
        public bool isFading;
        public float endTime;
        public MeshFrameDataBase fromFrame;
        public MeshFrameDataBase toFrame;
        public LerpMatrix4x4Job[] exposedTransformJobs;
        public JobHandle[] exposedTransformJobHandles;
        public NativeArray<Matrix4x4> fromMatrix;
        public NativeArray<Matrix4x4> toMatrix;
        public NativeArray<Matrix4x4>[] outputMatrix;

        private bool isReset;

        public void Reset(bool destroying)
        {
            if (!isReset)
            {
                ReturnFrame(destroying);
                isFading = false;
                endTime = 0;
                currentFrame = 0;
                framesNeeded = 0;
                fromFrame = null;
                toFrame = null;
                isReset = true;
            }
        }
        public void StartCrossfade(MeshFrameDataBase fromFrame, MeshFrameDataBase toFrame)
        {
            Reset(false);
            isReset = false;
            this.fromFrame = fromFrame;
            this.toFrame = toFrame;
            int matrixLength = fromFrame.exposedTransforms != null ? fromFrame.exposedTransforms.Length : 0;
            if (matrixLength > 0)
            {
                if (exposedTransformJobs == null) exposedTransformJobs = AllocatedArray<LerpMatrix4x4Job>.Get(framesNeeded);
                if (exposedTransformJobHandles == null) exposedTransformJobHandles = AllocatedArray<JobHandle>.Get(framesNeeded);
                if (outputMatrix == null) outputMatrix = AllocatedArray<NativeArray<Matrix4x4>>.Get(framesNeeded);
                fromMatrix = new NativeArray<Matrix4x4>(matrixLength, Allocator.Persistent);
                toMatrix = new NativeArray<Matrix4x4>(matrixLength, Allocator.Persistent);
                fromMatrix.CopyFrom(fromFrame.exposedTransforms);
                toMatrix.CopyFrom(toFrame.exposedTransforms);

                for (int i = 0; i < framesNeeded; i++)
                {
                    float delta = i / (float)framesNeeded;
                    outputMatrix[i] = new NativeArray<Matrix4x4>(matrixLength, Allocator.Persistent);
                    var matrixJob = new LerpMatrix4x4Job()
                    {
                        from = fromMatrix,
                        to = toMatrix,
                        output = outputMatrix[i],
                        delta = delta
                    };
                    exposedTransformJobs[i] = matrixJob;
                    exposedTransformJobHandles[i] = matrixJob.Schedule(matrixLength, 64);
                }
            }

            
        }
        public void ReturnFrame(bool destroying)
        {
            try
            {
                if (exposedTransformJobHandles != null)
                {
                    for (int i = 0; i < exposedTransformJobHandles.Length; i++)
                    {
                        if (destroying || currentFrame <= i)
                        {
                            exposedTransformJobHandles[i].Complete();
                        }
                    }
                    AllocatedArray.Return(exposedTransformJobHandles, true);
                    exposedTransformJobHandles = null;
                }
                if (exposedTransformJobs != null)
                {
                    AllocatedArray.Return(exposedTransformJobs, true);
                    exposedTransformJobs = null;
                }
                if (outputMatrix != null)
                {
                    if (fromMatrix.IsCreated)
                        fromMatrix.Dispose();
                    if (toMatrix.IsCreated)
                        toMatrix.Dispose();
                    for (int i = 0; i < outputMatrix.Length; i++)
                    {
                        if (outputMatrix[i].IsCreated)
                            outputMatrix[i].Dispose();
                    }
                    AllocatedArray.Return(outputMatrix);
                    outputMatrix = null;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
            }
        }
    }
}
