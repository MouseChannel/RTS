using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using System;
using Unity.Entities;

public interface IStateBaseJob : IJob
{

    void EnterState(DoingTaskState newState);
    void ExitState(DoingTaskState currentState);

    void ChangeState(DoingTaskState newState);


    void EcbSetComponent<T>(T component) where T : unmanaged, IComponentData;
    void EcbAddComponent<T>(T component) where T : unmanaged, IComponentData;
    void EcbRemoveComponent<T>(T component) where T : unmanaged, IComponentData;

}
 
