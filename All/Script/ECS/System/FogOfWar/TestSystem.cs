using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Entities;

// [AlwaysUpdateSystem]
[DisableAutoCreation]
public class TestSystem : SystemBase
{
    protected override void OnUpdate()
    {
        NativeArray<Color32> a = new NativeArray<Color32>(3, Allocator.Temp);
        
           a[0] = Change( a[0], 22);
         
        

        // Debug.Log(a[0]);

        a.Dispose();

    }
    public static Color32 Change( Color32 a  , byte r){
        // var b = a;

        // b.r = r;
        a.r = r;
        
        return a;
    }
}
