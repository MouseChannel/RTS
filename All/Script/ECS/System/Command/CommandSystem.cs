using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;
using Unity.Burst;
[UpdateInGroup(typeof(InitializationSystemGroup))]
[UpdateAfter(typeof(ResponseNetSystem))]

public partial class CommandSystem : WorkSystem
{

    public override void Work()
    {
        if (!ShouldRunSystem()) return;
       
        NativeList<JobHandle> jobList = new NativeList<JobHandle>(Allocator.Temp);
        Entities.ForEach((Entity entity, int entityInQueryIndex, in HasCommandState hasCommandState, in InhabitantComponent inhabitantComponent) =>
        {

            CommandJob commandJob = new CommandJob
            {
                entity = entity,
                hasCommandState = hasCommandState,
                inhabitantComponent = inhabitantComponent,
                entityInQueryIndex = entityInQueryIndex,
                ecbPara = ecbPara
            };
            jobList.Add(commandJob.Schedule());

        }).WithoutBurst().Run();
        JobHandle.CompleteAll(jobList);
        jobList.Dispose();
    }


    [BurstCompile]
    public struct CommandJob : IJob
    {
        public Entity entity;
        [ReadOnly] public HasCommandState hasCommandState;
        public InhabitantComponent inhabitantComponent;
        public int entityInQueryIndex;
        [NativeDisableContainerSafetyRestriction]
        public EntityCommandBuffer.ParallelWriter ecbPara;


        public void Execute()
        {
            ecbPara.RemoveComponent<HasCommandState>(entityInQueryIndex, entity);
            switch (hasCommandState.type)
            {
                case CommandType.move:
                    int endPosition = hasCommandState.commandData;
                    ChangeInhabitantTaskType(InhabitantTaskType.Idle);
                    ecbPara.AddComponent<PathFindParam>(entityInQueryIndex, entity, new PathFindParam { endPosition = endPosition });

                    break;
                case CommandType.collect:
                    ChangeInhabitantTaskType(InhabitantTaskType.Collect);
                    break;
                case CommandType.fight:
                    ChangeInhabitantTaskType(InhabitantTaskType.Fight);
                    break;
            }
            ecbPara.SetComponent<InhabitantComponent>(entityInQueryIndex, entity, inhabitantComponent);


        }


        public void ChangeInhabitantTaskType(InhabitantTaskType newState)
        {
            // var entity = allMovedUnit[entityNo];

            ExitInhabitantState(inhabitantComponent.taskType);
            inhabitantComponent.taskType = newState;
            inhabitantComponent.taskState = DoingTaskState.idle;
            EnterInhabitantState(newState);


        }

        private void EnterInhabitantState(InhabitantTaskType newState)
        {
            switch (newState)
            {
                case InhabitantTaskType.Idle:
                    break;
                case InhabitantTaskType.Build:
                    break;
                case InhabitantTaskType.Fight:
                    // ecbPara.SetComponent<Fighter>(entityInQueryIndex, entity, new Fighter { });
                    ecbPara.AddComponent<DoingFight>(entityInQueryIndex, entity, new DoingFight { enemyNo = hasCommandState.commandData });

                    break;
                case InhabitantTaskType.Collect:

                    // ecbPara.SetComponent<Collector>(entityInQueryIndex, entity, new Collector { });
                    ecbPara.AddComponent<DoingCollect>(entityInQueryIndex, entity, new DoingCollect { resourceNo = hasCommandState.commandData });
                    break;
            }
        }
        private void ExitInhabitantState(InhabitantTaskType oldState)
        {
            switch (oldState)
            {
                case InhabitantTaskType.Idle:
                    break;
                case InhabitantTaskType.Build:
                    break;
                case InhabitantTaskType.Fight:
                    ecbPara.RemoveComponent<DoingFight>(entityInQueryIndex, entity);
                    break;
                case InhabitantTaskType.Collect:
                    ecbPara.RemoveComponent<DoingCollect>(entityInQueryIndex, entity);

                    break;
            }
        }
    }
}
