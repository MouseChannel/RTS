using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using FixedMath;
using RVO;
using Vector2 = RVO.Vector2;
public class UnitConvert : MonoBehaviour, IConvertGameObjectToEntity
{
    public Entity entity;
    private ResponseCommandSystem responseCommandSystem;
    void IConvertGameObjectToEntity.Convert(Entity entity_, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        entity = entity_;
        dstManager.AddComponentData<Agent>(entity,new Agent{
            id_ = Root.Instance.id,
            neighborDist_ = 5,
            maxNeighbors_ = 10,
            timeHorizon_ = 1,
            timeHorizonObst_ = 1,
            radius_ = ((FixedInt)1) >>1,
            maxSpeed_ = 6,
            velocity_ = new Vector2(0,0),
            position_ = new Vector2(Root.Instance.id, Root.Instance.id),
            // faction_ = Root.Instance.id,
            // needCheckClosestEnemy_ = true
            // needCheckRangeNeighbor = true,
        });
        

        dstManager.AddBuffer<PathPosition>(entity);
        dstManager.AddComponentData<PathFollow>(entity, new PathFollow{pathIndex = -1});
        dstManager.AddComponent<CanBeSelected>(entity);
        dstManager.AddComponent<UnitTag>(entity);
        dstManager.SetComponentData<UnitTag>(entity, new UnitTag{id = Root.Instance.id,faction = 1});
        responseCommandSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<ResponseCommandSystem>();
        responseCommandSystem.needMovedUnit.Add(entity);


        Root.Instance.id ++;
    }

    
}
