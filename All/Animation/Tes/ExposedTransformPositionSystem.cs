using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine.Jobs;

struct TransformCrossfadeJobParallel : IJobParallelFor
{
    public void Execute(int index)
    {

    }
}

public partial class ExposedTransformPositionSystem : SystemBase
{
    protected override void OnStartRunning()
    {
        // BlobAssetUtil.AddAnimationElement();
    }
    protected override void OnUpdate()
    {

        new MyJoo
        {

        }.Schedule().Complete();

    }


    private struct MyJoo : IJob
    {
        public int i;
        public void Execute()
        {
            if (BlobAssetUtil.animationBlobDic.ContainsKey("IdleManBored"))
            {
                Debug.Log(string.Format("{0}", BlobAssetUtil.animationBlobDic["IdleManBored"].Value.exposedFramePositionData[0].singleFrameData[0].translation));

            }
        }
    }
}


