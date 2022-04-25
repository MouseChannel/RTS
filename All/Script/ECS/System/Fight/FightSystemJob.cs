using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using System;
using FixedMath;

public partial class FightSystem
{
    [BurstCompile]
    public struct FightJob : IStateBaseJob
    {
        public Entity entity;
        public int entityInQueryIndex;
        // public Fighter inhabitantComponent;
        public InhabitantComponent inhabitantComponent;
        public FixedVector2 fighterPosition;
        public FixedVector2 enemyPosition;

        [NativeDisableContainerSafetyRestriction]
        public EntityCommandBuffer.ParallelWriter ecbPara;
        private int fighterPositionIndex;
        private int enemyPositionIndex;
        private int beforeEnemyPositionIndex;

        public void Execute()
        {
            fighterPositionIndex = GridSystem.GetGridIndex(fighterPosition);
            enemyPositionIndex = GridSystem.GetGridIndex(enemyPosition);
            beforeEnemyPositionIndex = inhabitantComponent.firstValue;

            // Debug.Log(string.Format("{0}  {1}", fighterPositionIndex, enemyPositionIndex));
            switch (inhabitantComponent.taskState)
            {
                case DoingTaskState.idle:
                    Case_Idle();
                    break;
                case DoingTaskState.goToDestination:
                    Case_ChaseEnemy();
                    break;
                case DoingTaskState.working:
                    Case_Fight();
                    break;
            }

        }

        private void Case_Idle()
        {
            if (enemyPositionIndex != FixedInt.inVaild)
            {
                ChangeState(DoingTaskState.goToDestination);
            }

        }
        private void Case_ChaseEnemy()
        {
            if (enemyPositionIndex == FixedInt.inVaild)
            {
                ChangeState(DoingTaskState.idle);
                return;
            }
            if (InAround())
            {
                ChangeState(DoingTaskState.working);
            }
            else if (beforeEnemyPositionIndex != enemyPositionIndex)
            {
                inhabitantComponent.firstValue = enemyPositionIndex;
                EcbSetComponent(inhabitantComponent);
                EcbAddComponent(new PathFindParam
                {
                    endPosition = enemyPositionIndex
                });




            }

        }
        private void Case_Fight()
        {
            if (enemyPositionIndex == FixedInt.inVaild)
            {
                ChangeState(DoingTaskState.idle);
                return;
            }
            if (!InAround())
            {
                ChangeState(DoingTaskState.goToDestination);
            }

        }





        private bool InAround()
        {
            int a = fighterPositionIndex;
            int b = enemyPositionIndex;
            if (a == b + 1
                || a == b - 1
                || a == b + StaticData.gridWidth
                || a == b - StaticData.gridWidth)
            {
                return true;
            }
            return false;
        }





        public void EnterState(DoingTaskState newState)
        {
            switch (newState)
            {
                case DoingTaskState.idle:
                    break;
                case DoingTaskState.goToDestination:
                    break;
                case DoingTaskState.working:
                    EcbSetComponent(new CurrentPathIndex
                    {
                        pathIndex = -1
                    });



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
            }
        }

        public void ChangeState(DoingTaskState newState)
        {
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


}
