using System.Collections.Concurrent;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

unsafe public static class AllocateNativeArrayPool
{




    // public static void* GetNativeArrayPrt<T>(this NativeArray<T> arr) where T : struct
    // {
    //     return NativeArrayUnsafeUtility.GetUnsafePtr(arr);

    // }

    public static void PullArray<T>(int size, out NativeArray<T> array, out UIntPtr ptr) where T : struct
    {
        AllocateNativeArrayPool<T>.PullArray(size, out array, out ptr);
    }


    public static void GiveBackToPool<T>(T type, UIntPtr pointer) where T : struct
    {

        AllocateNativeArrayPool<T>.GiveBackToPool(pointer);
    }
    public static void Release<T>(T type, UIntPtr pointer) where T : struct
    {
        AllocateNativeArrayPool<T>.Release(pointer);
    }
}

unsafe public static class AllocateNativeArrayPool<T> where T : struct
{
    private static ConcurrentDictionary<UIntPtr, NativeArray<T>> cachedArray = new ConcurrentDictionary<UIntPtr, NativeArray<T>>();
    private static ConcurrentDictionary<UIntPtr, NativeArray<T>> arrayInUse = new ConcurrentDictionary<UIntPtr, NativeArray<T>>();



    // private static ConcurrentBag<NativeArray<T>> cachedArray = new ConcurrentBag<NativeArray<T>>(); 
    // private static List<NativeArray<T>> arrayInUse = new List<NativeArray<T>>();
    private static NativeArray<T> NewArray(int size)
    {



        var newArray = new NativeArray<T>(size, Allocator.Persistent);
        var uIntPtr = (UIntPtr)newArray.GetUnsafePtr();
        Debug.Log(uIntPtr.ToUInt64());
        arrayInUse.TryAdd(uIntPtr, newArray);
        Debug.Log(arrayInUse.Count);
        // arrayInUse.Add(newArray);
        return newArray;
    }
    public static void PullArray(int size, out NativeArray<T> array, out UIntPtr ptr)
    {
        foreach (var arr in cachedArray)
        {
            if (arr.Value.Length == size)
            {
                cachedArray.TryRemove(arr.Key, out var outputArr);
                arrayInUse.TryAdd(arr.Key, outputArr);
                array = arr.Value;
                ptr = arr.Key;
                // return outputArr;

            }

        }

        array = new NativeArray<T>(size, Allocator.Persistent);
        ptr = (UIntPtr)array.GetUnsafePtr();

        arrayInUse.TryAdd(ptr, array);
        // Debug.Log(arrayInUse.Count);
        // arrayInUse.Add(newArray);

        // return NewArray(size);
    }
    public static void LOOOO(){
        Debug.Log(cachedArray.Count + "  " + arrayInUse.Count);
    }

 
    public static void GiveBackToPool(void* pointer)
    {
        // lock (arrayInUse)
        // {
        //     for (int i = 0; i < arrayInUse.Count; i++)
        //     {
        //         var arrayPointer = arrayInUse[i].GetUnsafePtr();
        //         if (arrayPointer == pointer)
        //         {
        //             allocatedArrays.Add(arrayInUse[i]);
        //             arrayInUse.RemoveAt(i);

        //         }
        //     }
        // }

        GiveBackToPool((UIntPtr)pointer);
    }
 
    public static void GiveBackToPool(UIntPtr uIntPtr)
    {
       
        foreach (var i in arrayInUse)
        {
            Debug.Log(uIntPtr == i.Key);
        }
        arrayInUse.TryRemove(uIntPtr, out var arr);

        cachedArray.TryAdd(uIntPtr, arr);


    }

    public static void Release(void* pointer)
    {
 
        Release((UIntPtr)pointer);
    }

    public static void Release(UIntPtr uintPtr)
    {
        arrayInUse.TryRemove(uintPtr, out var arr);
        if(arr != null){
            arr.Dispose();
        }
        
    }



}

