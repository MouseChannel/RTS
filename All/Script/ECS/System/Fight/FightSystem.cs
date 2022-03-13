using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;

public partial class FightSystem : WorkSystem
{
    private ResponseNetSystem responseNetSystem;
     
    public override   void Work()
    {
       
        responseNetSystem = GetSystem<ResponseNetSystem>();
        NativeList<JobHandle> jobList = new NativeList<JobHandle>(Allocator.Temp);
        Entities.ForEach((Entity entity, int entityInQueryIndex, in DoingFight doingFight, in Agent agent, in Fighter fighter ) =>
        {
            FightJob fightJob = new FightJob
            {
                entity = entity,
                entityInQueryIndex = entityInQueryIndex,
                fighter = fighter,
                fighterPosition  = agent.position_,
                enemyPosition =  responseNetSystem.GetUnitEntityPosition(doingFight.enemyNo),

                // enemyPosition
                ecbPara = ecbPara,
                


            


            };
            jobList.Add(fightJob.Schedule());

        }).WithoutBurst().Run();

        JobHandle.CompleteAll(jobList);
        jobList.Dispose();
    }









}
