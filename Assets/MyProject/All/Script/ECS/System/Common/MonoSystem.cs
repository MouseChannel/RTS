using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class MonoSystem : MonoBehaviour
{
    protected T GetSystem<T>() where T : SystemBase
    {
        return World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<T>();
    }
}
