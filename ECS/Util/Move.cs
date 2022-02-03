using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using FixedMath;
using Unity.Mathematics;
using FixedMath;
using Pb;

using RVO;
public class Move : MonoBehaviour
{
    Entity entity;
    [SerializeField] Convert c;
    
    void Start()
    {
        BattleSystem.Instance.move += SetMoveParams;
        
    }

    public void SetMoveParams(int2 start, int2 end){
        entity = c.entity;
        
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        
        Agent agent = entityManager.GetComponentData<Agent>(entity);
        FixedVector3 startPos = new FixedVector3(agent.position_.x_,1,agent.position_.y_);

        GridInit.Instance.pathfindingGrid.GetXZ(startPos,out int startx, out int starty);
        entityManager.AddComponentData(entity,  new PathFindParams { 
        startPosition  = new int2(agent.position_.x_.RawInt, agent.position_.y_.RawInt),
        endPosition = new int2(end.x, end.y) 
        } );
    }
    void Update()
    {
        entity = c.entity;
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        var agent = entityManager.GetComponentData<Agent>(entity);
        var pos = agent.position_;
        var dir = agent.velocity_;
        // Debug.Log(dir);
        transform.position = Vector3.Lerp(transform.position,new Vector3(pos.x_.RawFloat,0,pos.y_.RawFloat), Time.deltaTime * 4 )  ;
        if(RVOMath.absSq(dir) > (FixedInt)0.001)
            transform.forward = Vector3.Lerp(transform.forward, new Vector3(dir.x_.RawFloat,0,dir.y_.RawFloat) , Time.deltaTime * 10 )  ;
      

    }
    private void ValidateGridPosition(ref int x, ref int y) {
        x = math.clamp(x, 0, GridInit.Instance.pathfindingGrid.GetWidth() - 1);
        y = math.clamp(y, 0, GridInit.Instance.pathfindingGrid.GetHeight() - 1);
    }


    
}
