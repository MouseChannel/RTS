using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;
using Unity.Burst;
public class CommandSystem : WorkSystem
{

    public override void Work()
    {
        NativeList<JobHandle> jobList = new NativeList<JobHandle>(Allocator.Temp);
        Entities.ForEach((Entity entity, int entityInQueryIndex, in HasCommand hasCommand, in InhabitantComponent inhabitantComponent) =>
        {

            CommandJob commandJob = new CommandJob
            {
                entity = entity,
                hasCommand = hasCommand,
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
        [ReadOnly] public HasCommand hasCommand;
        public InhabitantComponent inhabitantComponent;
        public int entityInQueryIndex;
        [NativeDisableContainerSafetyRestriction]
        public EntityCommandBuffer.ParallelWriter ecbPara;


        public void Execute()
        {
            ecbPara.RemoveComponent<HasCommand>(entityInQueryIndex, entity);
            switch (hasCommand.type)
            {
                case CommandType.move:
                    ChangeInhabitantState(InhabitantState.Idle);
                    ecbPara.AddComponent<PathFindParam>(entityInQueryIndex, entity, new PathFindParam { endPosition = hasCommand.commandData });

                    break;
                case CommandType.collect:
                    ChangeInhabitantState(InhabitantState.Collect);
                    break;
                case CommandType.fight:
                    ChangeInhabitantState(InhabitantState.Fight);
                    break;
            }

        }


        public void ChangeInhabitantState(InhabitantState newState)
        {
            // var entity = allMovedUnit[entityNo];

            ExitInhabitantState(inhabitantComponent.state);
            inhabitantComponent.state = newState;
            EnterInhabitantState(newState);

            ecbPara.SetComponent<InhabitantComponent>(entityInQueryIndex, entity, inhabitantComponent);

        }

        private void EnterInhabitantState(InhabitantState newState)
        {
            switch (newState)
            {
                case InhabitantState.Idle:
                    break;
                case InhabitantState.Build:
                    break;
                case InhabitantState.Fight:
                    ecbPara.SetComponent<Fighter>(entityInQueryIndex, entity, new Fighter { });
                    ecbPara.AddComponent<DoingFight>(entityInQueryIndex, entity, new DoingFight { enemyNo = hasCommand.commandData });

                    break;
                case InhabitantState.Collect:

                    ecbPara.SetComponent<Collector>(entityInQueryIndex, entity, new Collector { });
                    ecbPara.AddComponent<DoingCollect>(entityInQueryIndex, entity, new DoingCollect { resourceNo = hasCommand.commandData });
                    break;
            }
        }
        private void ExitInhabitantState(InhabitantState oldState)
        {
            switch (oldState)
            {
                case InhabitantState.Idle:
                    break;
                case InhabitantState.Build:
                    break;
                case InhabitantState.Fight:
                    ecbPara.RemoveComponent<DoingFight>(entityInQueryIndex, entity);
                    break;
                case InhabitantState.Collect:
                    ecbPara.RemoveComponent<DoingCollect>(entityInQueryIndex, entity);

                    break;
            }
        }
    }
}
