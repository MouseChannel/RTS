using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;

public partial class FightSystem : WorkSystem
{
     
    public override   void Work()
    {
        NativeList<JobHandle> jobList = new NativeList<JobHandle>(Allocator.Temp);
        Entities.ForEach((Entity entity, int entityInQueryIndex, in DoingFight doingFight, in Agent agent, in Fighter fighter ) =>
        {
            FightJob fightJob = new FightJob
            {
                entity = entity,
                entityInQueryIndex = entityInQueryIndex,
                fighter = fighter


            };

        }).WithoutBurst().Run();

        JobHandle.CompleteAll(jobList);
    }









}
