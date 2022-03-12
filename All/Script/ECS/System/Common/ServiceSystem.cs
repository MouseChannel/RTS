using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public abstract class ServiceSystem : SystemBase
{
    // Start is called before the first frame update
      protected T GetSystem<T>() where T: SystemBase{
        return World.GetOrCreateSystem<T>();
    }

}
