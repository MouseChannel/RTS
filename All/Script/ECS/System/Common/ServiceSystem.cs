using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public abstract partial class ServiceSystem : SystemBase
{

   
    protected T GetSystem<T>() where T : SystemBase
    {
        return World.GetOrCreateSystem<T>();
    }

}
