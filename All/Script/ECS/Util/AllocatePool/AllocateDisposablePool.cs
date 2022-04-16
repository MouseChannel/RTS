using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
unsafe public static class AllocateNativeArrayPool
{



    public static void* GetNativeArrayPrt<T>(this NativeArray<T> arr) where T : struct
    {
        return   NativeArrayUnsafeUtility.GetUnsafePtr(arr);

    }

    public static NativeArray<T> PullArray<T>(int size) where T : struct
    {
        return AllocateNativeArrayPool<T>.PullArray(size);
    }
    public static void GiveBackToPool<T>(NativeArray<T> array) where T : struct
    {
        AllocateNativeArrayPool<T>.GiveBackToPool(array);
    }
}

public static class AllocateNativeArrayPool<T> where T : struct 
{
    private static List<NativeArray<T>> allocatedArrays = new List<NativeArray<T>>();
    private static NativeArray<T> NewArray(int size)
    {
        return new NativeArray<T>(size, Allocator.Persistent);
    }
    public static NativeArray<T> PullArray(int size)
    {
        lock (allocatedArrays)
        {
            for (int i = 0; i < allocatedArrays.Count; i++)
            {
                var array = allocatedArrays[i];
                if (array.Length == size)
                {
                    allocatedArrays.RemoveAt(i);
                    return array;
                }
            }
        }
        return NewArray(size);
    }

    public static void GiveBackToPool(NativeArray<T> array)
    {
        lock (allocatedArrays)
        {
            allocatedArrays.Add(array);
        }

    }



}

