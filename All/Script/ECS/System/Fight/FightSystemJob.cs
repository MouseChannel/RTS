using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using System;

public partial class FightSystem
{
    [BurstCompile]
    public struct FightJob : IJob
    {
        public Entity entity;
        public int entityInQueryIndex;
        public Fighter fighter;

        public void Execute()
        {
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

        }
        private void Case_ChaseEnemy()
        {

        }
        private void Case_Fight()
        {
        }



    }


}
