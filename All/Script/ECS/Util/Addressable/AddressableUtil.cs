using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AddressableUtil
{
    public static void LoadAssetAsync<T>(Action<T> callback, string assetName)
    {
        var handle = Addressables.LoadAssetAsync<T>(assetName);
        handle.Completed += (AsyncOperationHandle<T> obj) =>
        {
            callback(obj.Result);
        };
     

    }
    public static void LoadAssetSync<T>(Action<T> callback, string assetName)
    {
        var handle = Addressables.LoadAssetAsync<T>(assetName);

        callback(handle.WaitForCompletion());
        
    }

    public static void Release<T>(T obj)
    {
        Addressables.Release<T>(obj);
    }


}
