using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

[UpdateInGroup(typeof(SimulationSystemGroup))]

public class TaskGroup : ComponentSystemGroup
{
   
}
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(TaskGroup))]
public class CommandGroup : ComponentSystemGroup
{
   
}


 
