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
    public struct CollectorJob : IJob
    {
        public Entity entity;
        public FixedVector2 collectorPosition;
        public Collector collector;
 
 
        public FixedVector2 resourcePosition ;
        public FixedVector2 stopPosition;

        [NativeDisableContainerSafetyRestriction]
        public EntityCommandBuffer.ParallelWriter ecbPara;
        public int entityInQueryIndex;


        private int collectorPositionIndex;
        private int resourcePositionIndex;
        private int stopPositionIndex;
        public void Execute()
        {
            collectorPositionIndex = GridSystem.GetGridIndex(collectorPosition);
            resourcePositionIndex = GridSystem.GetGridIndex(resourcePosition);
            stopPositionIndex = GridSystem.GetGridIndex(stopPosition);



            switch (collector.collectorState)
            {
                case CollectorState.idle:
                    Case_Idle();
                    break;
                case CollectorState.goToResource:
                    Case_GoToResource();
                    break;
                case CollectorState.working:
                    Case_Working();
                    break;
                case CollectorState.backToStop:
                    Case_BackToStop();
                    break;

            }
        }




        private void Case_Idle()
        {
        
            if (resourcePosition != FixedVector2.inVaild)
            {
                ChangeCollectorState(CollectorState.goToResource);
            }
            else
            {
                Debug.Log("Idle");
            }

        }
        private void Case_GoToResource()
        {
           
            if (resourcePosition  == FixedVector2.inVaild)
            {
                ChangeCollectorState(CollectorState.idle);

                return;
            }
            //Animation

            if (collectorPositionIndex == resourcePositionIndex)
            {
                ChangeCollectorState(CollectorState.working);
            }

        }
        private void Case_Working()
        {
          
            if (resourcePosition  == FixedVector2.inVaild)
            {
                if (collector.currentResourceStore < 20)
                {
                    ChangeCollectorState(CollectorState.idle);
                }
                else
                {
                    ChangeCollectorState(CollectorState.backToStop);
                }
                return;
            }
            if (collector.currentResourceStore > 200)
            {
                ChangeCollectorState(CollectorState.backToStop);
            }
            else
            {
                collector.currentResourceStore++;
                AddToEcbQueue();
            }





        }
        private void Case_BackToStop()
        {
    
            if (stopPosition  != FixedVector2.inVaild)
            {
                if (collectorPositionIndex ==  stopPositionIndex)
                {
                    ChangeCollectorState(CollectorState.idle);
                }
            }
            else
            {
                GetMainFortId(out int fortId);
               

                AddToEcbQueue();
            }

        }

        private void ChangeCollectorState(CollectorState targetState)
        {
            //animation todo
            ExitState(collector.collectorState);

            EnterState(targetState);
            collector.collectorState = targetState;
            AddToEcbQueue();

        }
        private void AddToEcbQueue() => ecbPara.SetComponent<Collector>(entityInQueryIndex, entity, collector);



        private void EnterState(CollectorState state)
        {
            switch (state)
            {
                case CollectorState.idle:

                    break;
                case CollectorState.goToResource:
                    ecbPara.AddComponent<PathFindParam>(entityInQueryIndex, entity, new PathFindParam { endPosition = resourcePositionIndex });

                    break;
                case CollectorState.working:

                    break;
                case CollectorState.backToStop:

                    ecbPara.AddComponent<PathFindParam>(entityInQueryIndex, entity, new PathFindParam { endPosition = stopPositionIndex });


                    break;
            }
        }
        private void ExitState(CollectorState state)
        {
            switch (state)
            {
                case CollectorState.idle:

                    break;
                case CollectorState.goToResource:

                    break;
                case CollectorState.working:

                    break;
                case CollectorState.backToStop:
                    collector.currentResourceStore = 0;
                    break;
            }

        }


    }

    public static void GetMainFortId(out int fortId)
    {
        fortId = 1;

        // EntityManager.GetComponentData<Obstacle>()
    }
}
