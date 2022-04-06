using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Burst;
using Unity.Jobs;
using System;
using FixedMath;
using Unity.Entities;
using Unity.Collections.LowLevel.Unsafe;

public partial class CollectorSystem
{
    [BurstCompile]
    public   struct CollectorJob : IStateBaseJob
    {
        public Entity entity;
        public FixedVector2 collectorPosition;
        // public Collector inhabitantComponent;
        public InhabitantComponent inhabitantComponent;


        public FixedVector2 resourcePosition;
        public FixedVector2 stopPosition;

        [NativeDisableContainerSafetyRestriction]
        public EntityCommandBuffer.ParallelWriter ecbPara;
        public int entityInQueryIndex;


        private int collectorPositionIndex;
        private int resourcePositionIndex;
        private int stopPositionIndex;
        
        private int currentResourceStore;
        public void Execute()
        {
            collectorPositionIndex = GridSystem.GetGridIndex(collectorPosition);
            resourcePositionIndex = GridSystem.GetGridIndex(resourcePosition);
            stopPositionIndex = GridSystem.GetGridIndex(stopPosition);
            currentResourceStore = inhabitantComponent.firstValue;



            switch (inhabitantComponent.taskState)
            {
                case DoingTaskState.idle:
                    Case_Idle();
                    break;
                case DoingTaskState.goToDestination:
                    Case_GoToDestination();
                    break;
                case DoingTaskState.working:
                    Case_Working();
                    break;
                case DoingTaskState.goToSecondDestination:
                    Case_GoToSecondDestination();
                    break;



            }
        }




        private void Case_Idle()
        {

            if (resourcePosition != FixedVector2.inVaild)
            {
                ChangeState(DoingTaskState.goToDestination);
            }
            else
            {
                Debug.Log("Idle");
            }

        }
        private void Case_GoToDestination()
        {

            if (resourcePosition == FixedVector2.inVaild)
            {
                ChangeState(DoingTaskState.idle);

                return;
            }
            //Animation

            if (collectorPositionIndex == resourcePositionIndex)
            {
                ChangeState(DoingTaskState.working);
            }

        }
        private void Case_Working()
        {

            if (resourcePosition == FixedVector2.inVaild)
            {
                if (currentResourceStore < 20)
                {
                    ChangeState(DoingTaskState.idle);
                }
                else
                {
                    ChangeState(DoingTaskState.goToSecondDestination);
                }
                return;
            }
            if (currentResourceStore > 200)
            {
                ChangeState(DoingTaskState.goToSecondDestination);
            }
            else
            {
 
                currentResourceStore++;
                inhabitantComponent.firstValue = currentResourceStore;
                EcbSetComponent(inhabitantComponent);
             
            }





        }
        private void Case_GoToSecondDestination()
        {

            if (stopPosition != FixedVector2.inVaild)
            {
                if (collectorPositionIndex == stopPositionIndex)
                {
                    ChangeState(DoingTaskState.idle);
                }
            }
            else
            {
                GetMainFortId(out int fortId);


                // EcbSetComponent();
            }

        }



        public void EnterState(DoingTaskState newState)
        {
            switch (newState)
            {
                case DoingTaskState.idle:

                    break;
                case DoingTaskState.goToDestination:
                    EcbAddComponent(new PathFindParam
                    {
                        endPosition = resourcePositionIndex
                    });
                    





                    break;
                case DoingTaskState.working:

                    break;
                case DoingTaskState.goToSecondDestination:
                    EcbAddComponent(new PathFindParam
                    {
                        endPosition = stopPositionIndex
                    });
                     

                    // ecbPara.AddComponent<PathFindParam>(entityInQueryIndex, entity, new PathFindParam { endPosition = stopPositionIndex });


                    break;
            }
        }

        public void ExitState(DoingTaskState currentState)
        {
            switch (currentState)
            {
                case DoingTaskState.idle:

                    break;
                case DoingTaskState.goToDestination:

                    break;
                case DoingTaskState.working:

                    break;
                case DoingTaskState.goToSecondDestination:
                    inhabitantComponent.firstValue = 0;
                    break;
            }
        }

        public void ChangeState(DoingTaskState newState)
        {
            //animation todo
            ExitState(inhabitantComponent.taskState);

            EnterState(newState);
            inhabitantComponent.taskState = newState;
           
            EcbSetComponent(inhabitantComponent);
        }

 

 

        public void EcbSetComponent<T>(T component) where T : struct, IComponentData
        {
             ecbPara.SetComponent<T>(entityInQueryIndex, entity, component);
        }

        public void EcbAddComponent<T>(T component) where T : struct, IComponentData
        {
           ecbPara.AddComponent<T>(entityInQueryIndex, entity, component);
        }

        public void EcbRemoveComponent<T>(T component) where T : struct, IComponentData
        {
           ecbPara.RemoveComponent<T>(entityInQueryIndex, entity);
        }
    }

    public static void GetMainFortId(out int fortId)
    {
        fortId = 1;

        // EntityManager.GetComponentData<Obstacle>()
    }
}
