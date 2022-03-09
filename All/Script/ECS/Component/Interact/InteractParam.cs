using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public enum InteractType{
    Fight = 1,
    Build = 2,
    Collect = 3,
}
public struct CollectCommand : IComponentData{
    public ResourceComponent resource;
}

public struct DoingCollectTag : IComponentData
{
    
}
