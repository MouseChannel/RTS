using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using FixedMath;
 
 
public class UnitConvert : MonoSystem, IConvertGameObjectToEntity
{
     
  
    // private SelectionSystem selectionSystem;
    void IConvertGameObjectToEntity.Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        

        // responseCommandSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<ResponseCommandSystem>();
        dstManager.AddComponentData<Agent>(entity, new Agent
        {
            id_ = GetSystem<ResponseNetSystem>().allMovedUnit.Count,
            neighborDist_ = 5,
            maxNeighbors_ = 10,
            timeHorizon_ = 1,
            timeHorizonObst_ = 1,
            radius_ = ((FixedInt)1) >> 1,
            maxSpeed_ = 6,
            velocity_ = new FixedVector2(0, 0),
            position_ = new FixedVector2(GetSystem<ResponseNetSystem>().allMovedUnit.Count + 11, GetSystem<ResponseNetSystem>().allMovedUnit.Count+ 11),
            // position_ = new FixedVector2(24, 10)
        // faction_ = Root.Instance.id,
        // needCheckClosestEnemy_ = true
        // needCheckRangeNeighbor = true,

    });


        dstManager.AddBuffer<PathPosition>(entity);
        dstManager.AddComponentData<CurrentPathIndex>(entity, new CurrentPathIndex { pathIndex = -1 });
        dstManager.AddComponent<FOWUnit>(entity);
        
        dstManager.SetComponentData<FOWUnit>(entity, new FOWUnit { });
        dstManager.AddComponent<InhabitantComponent>(entity);

        dstManager.AddComponent<CollectorTag>(entity);
        dstManager.AddComponent<FighterTag>(entity);
        // dstManager.AddComponent<UnitTag>(entity);
        // dstManager.SetComponentData<UnitTag>(entity, new UnitTag{id = Root.Instance.id,faction = 1});

        GetSystem<ResponseNetSystem>().allMovedUnit.Add(entity);


        
        transform.parent.GetComponent<ViewUnit>().entity = entity;
    }

    
}
