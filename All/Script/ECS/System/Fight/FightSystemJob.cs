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
    public struct FightJob : IJob
    {
        public Entity entity;
        public int entityInQueryIndex;
        public Fighter fighter;
        public FixedVector2 fighterPosition;
        public FixedVector2 enemyPosition;

        [NativeDisableContainerSafetyRestriction]
        public EntityCommandBuffer.ParallelWriter ecbPara;
        private int fighterPositionIndex;
        private int enemyPositionIndex;

        public void Execute()
        {
            fighterPositionIndex = GridSystem.GetGridIndex(fighterPosition);
            enemyPositionIndex = GridSystem.GetGridIndex(enemyPosition);

            Debug.Log(string.Format("{0}  {1}", fighterPositionIndex, enemyPositionIndex));
            switch (fighter.state)
            {
                case FighterState.idle:
                    Case_Idle();
                    break;
                case FighterState.chaseEnemy:
                    Case_ChaseEnemy();
                    break;
                case FighterState.fight:
                    Case_Fight();
                    break;
            }

        }

        private void Case_Idle()
        {
            if (enemyPositionIndex != FixedInt.inVaild)
            {
                ChangeFighterState(FighterState.chaseEnemy);
            }

        }
        private void Case_ChaseEnemy()
        {
            if (enemyPositionIndex == FixedInt.inVaild)
            {
                ChangeFighterState(FighterState.idle);
                return;
            }
            if (InAround())
            {
                ChangeFighterState(FighterState.fight);
            }
            else if (fighter.beforeEnemyPositionIndex != enemyPositionIndex)
            {
                fighter.beforeEnemyPositionIndex = enemyPositionIndex;
                AddToEcbQueue();
                ecbPara.AddComponent<PathFindParam>(enemyPositionIndex, entity, new PathFindParam { endPosition = enemyPositionIndex });

            }

        }
        private void Case_Fight()
        {
            if (enemyPositionIndex == FixedInt.inVaild)
            {
                ChangeFighterState(FighterState.idle);
                return;
            }
            if (!InAround())
            {
                ChangeFighterState(FighterState.chaseEnemy);
            }

        }

        private void ChangeFighterState(FighterState newState)
        {
            ExitState(fighter.state);
            EnterState(newState);
            fighter.state = newState;
            AddToEcbQueue();



        }
        private void EnterState(FighterState newState)
        {
            switch (newState)
            {
                case FighterState.idle:
                    break;
                case FighterState.chaseEnemy:
                    // ecbPara.AddComponent<PathFindParam>(enemyPositionIndex, entity, new PathFindParam { endPosition = enemyPositionIndex });
                    break;
                case FighterState.fight:
                    ecbPara.SetComponent<CurrentPathIndex>(enemyPositionIndex, entity, new CurrentPathIndex { pathIndex = -1 });
                    break;
            }
        }

        private void ExitState(FighterState currentState)
        {
            switch (currentState)
            {
                case FighterState.idle:
                    break;
                case FighterState.chaseEnemy:
                    break;
                case FighterState.fight:

                    break;
            }
        }
        private   bool InAround()
        {
            int a = fighterPositionIndex;
            int b = enemyPositionIndex;
            if (a == b + 1
                || a == b - 1
                || a == b + ConfigData.gridWidth
                || a == b - ConfigData.gridWidth)
            {
                return true;
            }
            return false;
        }
        private void AddToEcbQueue() => ecbPara.SetComponent<Fighter>(enemyPositionIndex, entity, fighter);



    }


}
