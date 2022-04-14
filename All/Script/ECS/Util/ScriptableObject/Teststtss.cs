using UnityEngine.AddressableAssets;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using System;
using UnityEngine.Profiling;

public class Teststtss : MonoBehaviour
{
    public AnimationScriptableObject anim;
    private UnitScriptableObject ee;
    int frame;


    // Start is called before the first frame update


    void Start()
    {
        Debug.Log(anim.animationName);
        Debug.Log(anim.exposedFramePositionData.Length);
        // Debug.Log(u.Archetype);

        // var handle = Addressables.LoadAssetAsync<UnitScriptableObject>("Unit");
        // handle.Completed += Solve;


        // Addressables.LoadAssetAsync<Texture2D>("MainTexture").Completed += Solve;


        // textureHandle.Completed += TextureHandle_Completed;
        // AddressableUtil.Release<Uni>
    }

    private void Solve(UnitScriptableObject obj)
    {
         ee = obj;
     Debug.Log(frame);
        Debug.Log(ee.animations[0].animScalar);
        
    }

    // private void Solve(AsyncOperationHandle obj)
    // {
    //     ee = obj;
    //     Debug.Log(obj ;
    // }

    // Update is called once per frame
    void Update()
    {
        frame++;
        if(Input.GetMouseButtonDown(0)){
            Debug.Log(frame);
            Profiler.BeginSample("loaddd");
            AddressableUtil.LoadAssetSync<UnitScriptableObject>(Solve, "Unit");
            Profiler.EndSample();
            // Debug.Log(ee.dimension);
            // ee = new AsyncOperationHandle();
        }
        if(Input.GetMouseButtonDown(1)){
            AddressableUtil.Release(ee);
        }

    }
}
