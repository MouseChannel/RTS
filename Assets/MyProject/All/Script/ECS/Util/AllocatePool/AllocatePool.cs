using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;

public static class AllocateArrayPool
{
    public static T[] PullArray<T>(int size) where T : struct
    {
        return AllocateArrayPool<T>.PullArray(size);
    }
    public static void GiveBackToPool<T>(T[] array) where T : struct
    {
        AllocateArrayPool<T>.GiveBackToPool(array);
    }

}
public static class AllocateArrayPool<T> where T : struct
{
    private static List<T[]> allocatedArrays = new List<T[]>();
    private static T[] NewArray(int size)
    {
        return new T[size];
    }
    public static T[] PullArray(int size)
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

    public static void GiveBackToPool(T[] array)
    {
        lock (allocatedArrays)
        {
            allocatedArrays.Add(array);
        }

    }



}
